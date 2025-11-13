using ApiContracts.DTOs;

namespace BlazorApp.Services;

public interface IPostService
{
    Task<PostDto> AddPostAsync(CreatePostDto request);
    Task UpdatePostAsync(int id, UpdatePostDto request);
    Task DeletePostAsync(int id);
    Task<PostDto?> GetPostByIdAsync(int id, bool includeAuthor = true, bool includeComments = true);
    Task<IReadOnlyCollection<PostDto>> GetPostsAsync(
        string? titleContains = null,
        int? authoredByUserId = null);
}