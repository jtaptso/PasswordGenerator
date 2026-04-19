using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<bool> ExistsAsync(string username, string email);
    Task<User?> GetByIdAsync(int id);
    Task UpdateAsync(User user);
}
