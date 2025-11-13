using ApiContracts.DTOs;

namespace BlazorApp.Services;

public interface IUserService
{
    Task<UserDto> AddUserAsync(CreateUserDto request);
    Task UpdateUserAsync(int id, UpdateUserDto request);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(string? userNameContains = null);
}