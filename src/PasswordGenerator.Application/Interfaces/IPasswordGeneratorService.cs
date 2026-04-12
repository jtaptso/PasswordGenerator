using PasswordGenerator.Application.DTOs;

namespace PasswordGenerator.Application.Interfaces;

public interface IPasswordGeneratorService
{
    GenerateResult GeneratePassword(GenerateRequest request);
    GenerateResult GeneratePassphrase(GenerateRequest request);
}
