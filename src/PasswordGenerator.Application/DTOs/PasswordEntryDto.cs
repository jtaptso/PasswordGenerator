namespace PasswordGenerator.Application.DTOs;

public class PasswordEntryDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
