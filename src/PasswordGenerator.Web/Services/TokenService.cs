namespace PasswordGenerator.Web.Services;

public class TokenService
{
    private string? _token;
    private DateTime _expiresAt;

    public string? Token => _token;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token) && _expiresAt > DateTime.UtcNow;

    public void SetToken(string token, DateTime expiresAt)
    {
        _token = token;
        _expiresAt = expiresAt;
    }

    public void ClearToken()
    {
        _token = null;
        _expiresAt = DateTime.MinValue;
    }
}
