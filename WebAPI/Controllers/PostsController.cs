using ApiContracts.Posts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("posts")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _posts;
    private readonly IUserRepository _users;

    public PostsController(IPostRepository posts, IUserRepository users)
    {
        _posts = posts;
        _users = users;
    }

    // POST /posts
    [HttpPost]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostDto dto)
    {
        // Optional integrity check: author must exist
        var author = await _users.GetByIdAsync(dto.UserId);
        if (author is null) return BadRequest($"User with id {dto.UserId} not found.");

        var entity = new Post
        {
            Title = dto.Title,
            Body  = dto.Body,
            UserId = dto.UserId
        };

        var created = await _posts.AddAsync(entity);
        var result = ToDto(created);
        return CreatedAtAction(nameof(GetSingle), new { id = result.Id }, result);
    }

    // GET /posts/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSingle(int id)
    {
        var post = await _posts.GetByIdAsync(id);
        if (post is null) return NotFound();

        return Ok(ToDto(post));
    }

    // GET /posts?titleContains=..&userId=..&userName=..
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetMany(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        var posts = await _posts.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(titleContains))
            posts = posts.Where(p =>
                p.Title.Contains(titleContains, StringComparison.OrdinalIgnoreCase));

        if (userId is not null)
            posts = posts.Where(p => p.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var matchingUserIds = (await _users.GetAllAsync())
                .Where(u => u.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase))
                .Select(u => u.Id)
                .ToHashSet();
            posts = posts.Where(p => matchingUserIds.Contains(p.UserId));
        }

        var list = posts.Select(ToDto).ToList();
        return Ok(list);
    }

    // PUT /posts/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePostDto dto)
    {
        var post = await _posts.GetByIdAsync(id);
        if (post is null) return NotFound();

        post.Title = dto.Title;
        post.Body  = dto.Body;

        await _posts.UpdateAsync(post);
        return NoContent();
    }

    // DELETE /posts/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _posts.GetByIdAsync(id);
        if (post is null) return NotFound();

        await _posts.DeleteAsync(id);
        return NoContent();
    }

    private static PostDto ToDto(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Body = p.Body,
        UserId = p.UserId
    };
}