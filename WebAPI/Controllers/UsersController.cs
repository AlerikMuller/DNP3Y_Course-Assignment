using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users)
    {
        _users = users;
    }

    // POST /users
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var entity = new User
        {
            UserName = dto.UserName,
            Password = dto.Password
        };

        var created = await _users.AddAsync(entity);
        var result = ToDto(created);
        return CreatedAtAction(nameof(GetSingle), new { id = result.Id }, result);
    }

    // GET /users/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetSingle(int id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return NotFound();

        return Ok(ToDto(user));
    }

    // GET /users?nameContains=ali
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetMany([FromQuery] string? nameContains)
    {
        var users = await _users.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            users = users.Where(u =>
                u.UserName.Contains(nameContains, StringComparison.OrdinalIgnoreCase));
        }

        var list = users.Select(ToDto).ToList();
        return Ok(list);
    }

    // PUT /users/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return NotFound();

        user.UserName = dto.UserName;
        await _users.UpdateAsync(user);
        return NoContent();
    }

    // DELETE /users/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null) return NotFound();

        await _users.DeleteAsync(id);
        return NoContent();
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        UserName = u.UserName
    };
}