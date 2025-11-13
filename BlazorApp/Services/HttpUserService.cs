using System.Net.Http.Json;
using System.Text.Json;
using ApiContracts.DTOs;
using BlazorApp.Services;

namespace BlazorApp.Services;

public class HttpUserService : IUserService
{
    private readonly HttpClient _client;

    public HttpUserService(HttpClient client)
    {
        _client = client;
    }

    public async Task<UserDto> AddUserAsync(CreateUserDto request)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("users", request);
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task UpdateUserAsync(int id, UpdateUserDto request)
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync($"users/{id}", request);
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        HttpResponseMessage response = await _client.GetAsync($"users/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(string? userNameContains = null)
    {
        string url = "users";
        if (!string.IsNullOrWhiteSpace(userNameContains))
        {
            url += $"?userNameContains={Uri.EscapeDataString(userNameContains)}";
        }

        HttpResponseMessage response = await _client.GetAsync(url);
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}