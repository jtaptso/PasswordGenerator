using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PasswordGenerator.Web.Services;

public class TokenService
{
    private string? _token;
    private DateTime _expiresAt;
    private List<string> _roles = [];

    public string? Token => _token;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token) && _expiresAt > DateTime.UtcNow;
    public IReadOnlyList<string> Roles => _roles;
    public bool IsAdmin => _roles.Contains("Admin");

    public void SetToken(string token, DateTime expiresAt)
    {
        _token = token;
        _expiresAt = expiresAt;
        _roles = ParseRoles(token);
    }

    public void ClearToken()
    {
        _token = null;
        _expiresAt = DateTime.MinValue;
        _roles = [];
    }

    private static List<string> ParseRoles(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();
    }
}
