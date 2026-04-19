using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.Application.Interfaces;

public interface IVaultRepository
{
    Task<List<PasswordEntry>> GetAllAsync(int userId);
    Task<PasswordEntry?> GetByIdAsync(int id, int userId);
    Task<PasswordEntry> AddAsync(PasswordEntry entry);
    Task<PasswordEntry> UpdateAsync(PasswordEntry entry);
    Task DeleteAsync(int id, int userId);
}
