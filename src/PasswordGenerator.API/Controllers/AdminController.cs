using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Application.DTOs;
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

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        var result = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = u.CreatedAt
        }).ToList();

        return Ok(result);
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

    [HttpDelete("users/{userId:int}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(int userId, string roleName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "User not found." });

        var existingRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name == roleName);
        if (existingRole is null)
            return BadRequest(new { message = $"User does not have the '{roleName}' role." });

        if (roleName == "User")
            return BadRequest(new { message = "Cannot remove the 'User' role." });

        user.UserRoles.Remove(existingRole);
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = $"Role '{roleName}' removed from user '{user.Username}'." });
    }

    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "User not found." });

        await _userRepository.DeleteAsync(userId);

        return Ok(new { message = $"User '{user.Username}' deleted." });
    }
}

public class AssignRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}
