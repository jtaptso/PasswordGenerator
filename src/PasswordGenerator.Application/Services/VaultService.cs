using PasswordGenerator.Application.DTOs;
using PasswordGenerator.Application.Interfaces;
using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.Application.Services;

public class VaultService
{
    private readonly IVaultRepository _repository;
    private readonly IEncryptionService _encryption;

    public VaultService(IVaultRepository repository, IEncryptionService encryption)
    {
        _repository = repository;
        _encryption = encryption;
    }

    public async Task<List<PasswordEntryDto>> GetAllAsync()
    {
        var entries = await _repository.GetAllAsync();
        return entries.Select(e => new PasswordEntryDto
        {
            Id = e.Id,
            Label = e.Label,
            Website = e.Website,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        }).ToList();
    }

    public async Task<PasswordEntryDto?> GetByIdAsync(int id)
    {
        var entry = await _repository.GetByIdAsync(id);
        if (entry is null) return null;

        return new PasswordEntryDto
        {
            Id = entry.Id,
            Label = entry.Label,
            Password = _encryption.Decrypt(entry.EncryptedPassword),
            Website = entry.Website,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }

    public async Task<PasswordEntryDto> AddAsync(PasswordEntryDto dto)
    {
        var entry = new PasswordEntry
        {
            Label = dto.Label,
            EncryptedPassword = _encryption.Encrypt(dto.Password ?? string.Empty),
            Website = dto.Website,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entry);

        return new PasswordEntryDto
        {
            Id = created.Id,
            Label = created.Label,
            Website = created.Website,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }

    public async Task<PasswordEntryDto?> UpdateAsync(int id, PasswordEntryDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Label = dto.Label;
        existing.Website = dto.Website;
        existing.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            existing.EncryptedPassword = _encryption.Encrypt(dto.Password);
        }

        var updated = await _repository.UpdateAsync(existing);

        return new PasswordEntryDto
        {
            Id = updated.Id,
            Label = updated.Label,
            Website = updated.Website,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
