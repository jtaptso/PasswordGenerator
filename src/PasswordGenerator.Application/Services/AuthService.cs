using PasswordGenerator.Application.DTOs;
using PasswordGenerator.Application.Interfaces;
using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.ExistsAsync(request.Username, request.Email);
        if (exists)
        {
            var errors = new List<string>();
            var byUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (byUsername is not null)
                errors.Add("Username is already taken.");

            var byEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (byEmail is not null)
                errors.Add("Email is already in use.");

            return new RegisterResult { Success = false, Errors = errors };
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserRoles = [new UserRole { RoleId = 2 }] // "User" role
        };

        var created = await _userRepository.AddAsync(user);

        return new RegisterResult { Success = true, UserId = created.Id };
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null) return null;

        return _passwordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
