using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private readonly string filePath = "users.json";

    public UserFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<User> AddAsync(User user)
    {
        string usersAsJson = await File.ReadAllTextAsync(filePath);
        List<User> users = JsonSerializer.Deserialize<List<User>>(usersAsJson);
        int maxId = users.Count > 0 ? users.Max(u => u.Id) : 1;
        user.Id = maxId + 1;
        users.Add(user);
        usersAsJson = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(filePath, usersAsJson);
        return user;
    }

    public IQueryable<User> GetMany()
    {
        string usersAsJson = File.ReadAllTextAsync(filePath).Result;
        List<User> users = JsonSerializer.Deserialize<List<User>>(usersAsJson);
        return users.AsQueryable();
    }
}