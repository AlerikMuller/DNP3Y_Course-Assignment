using System.Net.Http.Json;
using System.Text.Json;
using ApiContracts.DTOs;
using BlazorApp.Services;

namespace BlazorApp.Services;

public class HttpPostService : IPostService
{
    private readonly HttpClient _client;

    public HttpPostService(HttpClient client)
    {
        _client = client;
    }

    public async Task<PostDto> AddPostAsync(CreatePostDto request)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("posts", request);
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return JsonSerializer.Deserialize<PostDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task UpdatePostAsync(int id, UpdatePostDto request)
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync($"posts/{id}", request);
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);
    }

    public async Task DeletePostAsync(int id)
    {
        HttpResponseMessage response = await _client.DeleteAsync($"posts/{id}");
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);
    }

    public async Task<PostDto?> GetPostByIdAsync(int id, bool includeAuthor = true, bool includeComments = true)
    {
        string url = $"posts/{id}?includeAuthor={includeAuthor}&includeComments={includeComments}";
        HttpResponseMessage response = await _client.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return JsonSerializer.Deserialize<PostDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<IReadOnlyCollection<PostDto>> GetPostsAsync(string? titleContains = null, int? authoredByUserId = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(titleContains))
            queryParams.Add($"titleContains={Uri.EscapeDataString(titleContains)}");
        if (authoredByUserId != null)
            queryParams.Add($"userId={authoredByUserId.Value}");

        string url = "posts";
        if (queryParams.Any())
            url += "?" + string.Join("&", queryParams);

        HttpResponseMessage response = await _client.GetAsync(url);
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        return JsonSerializer.Deserialize<List<PostDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}