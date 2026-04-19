using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Application.DTOs;
using PasswordGenerator.Application.Services;

namespace PasswordGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaultController : ControllerBase
{
    private readonly VaultService _vaultService;

    public VaultController(VaultService vaultService)
    {
        _vaultService = vaultService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entries = await _vaultService.GetAllAsync(GetUserId());
        return Ok(entries);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entry = await _vaultService.GetByIdAsync(id, GetUserId());
        if (entry is null) return NotFound();
        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PasswordEntryDto dto)
    {
        var created = await _vaultService.AddAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PasswordEntryDto dto)
    {
        var updated = await _vaultService.UpdateAsync(id, dto, GetUserId());
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _vaultService.DeleteAsync(id, GetUserId());
        return NoContent();
    }
}
