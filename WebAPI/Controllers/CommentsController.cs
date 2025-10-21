using ApiContracts.Comments;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _comments;
    private readonly IUserRepository _users;
    private readonly IPostRepository _posts;

    public CommentsController(ICommentRepository comments, IUserRepository users, IPostRepository posts)
    {
        _comments = comments;
        _users = users;
        _posts = posts;
    }

    // POST /comments
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CreateCommentDto dto)
    {
        // Optional integrity checks
        if (await _users.GetByIdAsync(dto.UserId) is null)
            return BadRequest($"User with id {dto.UserId} not found.");
        if (await _posts.GetByIdAsync(dto.PostId) is null)
            return BadRequest($"Post with id {dto.PostId} not found.");

        var entity = new Comment
        {
            Body = dto.Body,
            UserId = dto.UserId,
            PostId = dto.PostId
        };

        var created = await _comments.AddAsync(entity);
        var result = ToDto(created);
        return CreatedAtAction(nameof(GetSingle), new { id = result.Id }, result);
    }

    // GET /comments/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetSingle(int id)
    {
        var c = await _comments.GetByIdAsync(id);
        if (c is null) return NotFound();

        return Ok(ToDto(c));
    }

    // GET /comments?postId=..&userId=..&userName=..
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetMany(
        [FromQuery] int? postId,
        [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        var comments = await _comments.GetAllAsync();

        if (postId is not null)
            comments = comments.Where(c => c.PostId == postId.Value);

        if (userId is not null)
            comments = comments.Where(c => c.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var matchingUserIds = (await _users.GetAllAsync())
                .Where(u => u.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase))
                .Select(u => u.Id)
                .ToHashSet();

            comments = comments.Where(c => matchingUserIds.Contains(c.UserId));
        }

        var list = comments.Select(ToDto).ToList();
        return Ok(list);
    }

    // PUT /comments/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
    {
        var c = await _comments.GetByIdAsync(id);
        if (c is null) return NotFound();

        c.Body = dto.Body;
        await _comments.UpdateAsync(c);
        return NoContent();
    }

    // DELETE /comments/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _comments.GetByIdAsync(id);
        if (c is null) return NotFound();

        await _comments.DeleteAsync(id);
        return NoContent();
    }

    private static CommentDto ToDto(Comment c) => new()
    {
        Id = c.Id,
        Body = c.Body,
        UserId = c.UserId,
        PostId = c.PostId
    };
}