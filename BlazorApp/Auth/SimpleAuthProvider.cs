using System.Security.Claims;
using System.Text.Json;
using ApiContracts.Auth;
using ApiContracts.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace BlazorApp.Auth;

public class SimpleAuthProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    private const string CurrentUserKey = "currentUser";

    public SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string userAsJson;
        try
        {
            userAsJson = await _jsRuntime.InvokeAsync<string>(
                "sessionStorage.getItem", CurrentUserKey);
        }
        catch (InvalidOperationException)
        {
            // JS runtime not ready yet
            return new AuthenticationState(new ClaimsPrincipal());
        }

        if (string.IsNullOrEmpty(userAsJson))
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        UserDto? userDto = JsonSerializer.Deserialize<UserDto>(userAsJson);
        if (userDto is null)
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        var claimsPrincipal = CreateClaimsPrincipal(userDto);
        return new AuthenticationState(claimsPrincipal);
    }

    public async Task LoginAsync(string userName, string password)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest(userName, password));

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(content);
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        string serialised = JsonSerializer.Serialize(userDto);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", CurrentUserKey, serialised);

        ClaimsPrincipal principal = CreateClaimsPrincipal(userDto);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", CurrentUserKey, "");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(UserDto userDto)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userDto.UserName),
            new(ClaimTypes.NameIdentifier, userDto.Id.ToString())
            // Add more custom claims based on UserDto if needed
        };

        var identity = new ClaimsIdentity(claims, "apiauth");
        return new ClaimsPrincipal(identity);
    }
}