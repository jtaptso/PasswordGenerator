using Microsoft.AspNetCore.DataProtection;
using PasswordGenerator.Application.Interfaces;

namespace PasswordGenerator.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("PasswordGenerator.Vault");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return _protector.Unprotect(cipherText);
    }
}
