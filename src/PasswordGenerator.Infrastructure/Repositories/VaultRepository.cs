using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Application.Interfaces;
using PasswordGenerator.Domain.Entities;
using PasswordGenerator.Infrastructure.Data;

namespace PasswordGenerator.Infrastructure.Repositories;

public class VaultRepository : IVaultRepository
{
    private readonly AppDbContext _context;

    public VaultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PasswordEntry>> GetAllAsync()
    {
        return await _context.PasswordEntries
            .OrderByDescending(e => e.UpdatedAt)
            .ToListAsync();
    }

    public async Task<PasswordEntry?> GetByIdAsync(int id)
    {
        return await _context.PasswordEntries.FindAsync(id);
    }

    public async Task<PasswordEntry> AddAsync(PasswordEntry entry)
    {
        _context.PasswordEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<PasswordEntry> UpdateAsync(PasswordEntry entry)
    {
        _context.PasswordEntries.Update(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _context.PasswordEntries.FindAsync(id);
        if (entry is not null)
        {
            _context.PasswordEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
