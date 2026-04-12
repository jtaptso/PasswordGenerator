namespace PasswordGenerator.Application.DTOs;

public class GenerateRequest
{
    public int Length { get; set; } = 16;
    public bool IncludeUppercase { get; set; } = true;
    public bool IncludeLowercase { get; set; } = true;
    public bool IncludeDigits { get; set; } = true;
    public bool IncludeSpecial { get; set; } = true;
    public bool ExcludeAmbiguous { get; set; } = false;
    public bool PassphraseMode { get; set; } = false;
    public int WordCount { get; set; } = 4;
    public string Separator { get; set; } = "-";
}
