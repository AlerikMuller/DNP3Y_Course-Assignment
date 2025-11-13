using System.Threading.Tasks;
using ApiContracts.Auth;
using ApiContracts.DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;

    public AuthController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
    {
        // Find user by username. Adjust to your own repo query logic.
        var usersQuery = _userRepo.GetMany();
        var user = usersQuery.SingleOrDefault(u => u.UserName == request.UserName);

        if (user is null || user.Password != request.Password)
        {
            // Incorrect username or password
            return Unauthorized("Invalid username or password");
        }

        var dto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName
            // Add other public fields if your UserDto has them
        };

        return Ok(dto);
    }
}