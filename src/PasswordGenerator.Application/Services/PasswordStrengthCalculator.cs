using PasswordGenerator.Domain.Enums;

namespace PasswordGenerator.Application.Services;

public static class PasswordStrengthCalculator
{
    public static double CalculateEntropy(string password)
    {
        if (string.IsNullOrEmpty(password))
            return 0;

        int poolSize = 0;
        bool hasLower = false, hasUpper = false, hasDigit = false, hasSpecial = false;

        foreach (var c in password)
        {
            if (char.IsLower(c)) hasLower = true;
            else if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }

        if (hasLower) poolSize += 26;
        if (hasUpper) poolSize += 26;
        if (hasDigit) poolSize += 10;
        if (hasSpecial) poolSize += 32;

        if (poolSize == 0) return 0;

        return password.Length * Math.Log2(poolSize);
    }

    public static PasswordStrength GetStrength(double entropy)
    {
        return entropy switch
        {
            < 28 => PasswordStrength.VeryWeak,
            < 36 => PasswordStrength.Weak,
            < 60 => PasswordStrength.Fair,
            < 80 => PasswordStrength.Strong,
            _ => PasswordStrength.VeryStrong
        };
    }
}
