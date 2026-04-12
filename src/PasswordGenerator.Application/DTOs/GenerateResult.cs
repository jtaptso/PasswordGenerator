using PasswordGenerator.Domain.Enums;

namespace PasswordGenerator.Application.DTOs;

public class GenerateResult
{
    public string Password { get; set; } = string.Empty;
    public double Entropy { get; set; }
    public PasswordStrength Strength { get; set; }
}
