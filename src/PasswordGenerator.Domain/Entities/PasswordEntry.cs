namespace PasswordGenerator.Domain.Entities;

public class PasswordEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Label { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
