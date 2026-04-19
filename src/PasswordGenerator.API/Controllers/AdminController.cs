using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Application.Interfaces;
using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("users/{userId:int}/roles")]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignRoleRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "User not found." });

        var existingRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name == request.RoleName);
        if (existingRole is not null)
            return BadRequest(new { message = $"User already has the '{request.RoleName}' role." });

        // Map role name to ID (Admin=1, User=2)
        var roleId = request.RoleName switch
        {
            "Admin" => 1,
            "User" => 2,
            _ => 0
        };

        if (roleId == 0)
            return BadRequest(new { message = $"Role '{request.RoleName}' does not exist." });

        user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = $"Role '{request.RoleName}' assigned to user '{user.Username}'." });
    }
}

public class AssignRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}
