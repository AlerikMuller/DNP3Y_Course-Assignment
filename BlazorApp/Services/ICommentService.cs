using ApiContracts.DTOs;

namespace BlazorApp.Services;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(CreateCommentDto request);
    Task<IReadOnlyCollection<CommentDto>> GetCommentsForPostAsync(int postId);
}