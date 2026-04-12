using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.Application.Interfaces;

public interface IVaultRepository
{
    Task<List<PasswordEntry>> GetAllAsync();
    Task<PasswordEntry?> GetByIdAsync(int id);
    Task<PasswordEntry> AddAsync(PasswordEntry entry);
    Task<PasswordEntry> UpdateAsync(PasswordEntry entry);
    Task DeleteAsync(int id);
}
