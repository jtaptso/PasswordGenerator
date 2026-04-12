using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Application.DTOs;
using PasswordGenerator.Application.Interfaces;
using PasswordGenerator.Application.Services;

namespace PasswordGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GeneratorController : ControllerBase
{
    private readonly IPasswordGeneratorService _generatorService;

    public GeneratorController(IPasswordGeneratorService generatorService)
    {
        _generatorService = generatorService;
    }

    [HttpPost("password")]
    public IActionResult GeneratePassword([FromBody] GenerateRequest request)
    {
        var result = _generatorService.GeneratePassword(request);
        return Ok(result);
    }

    [HttpPost("passphrase")]
    public IActionResult GeneratePassphrase([FromBody] GenerateRequest request)
    {
        var result = _generatorService.GeneratePassphrase(request);
        return Ok(result);
    }

    [HttpPost("strength")]
    public IActionResult CalculateStrength([FromBody] string password)
    {
        var entropy = PasswordStrengthCalculator.CalculateEntropy(password);
        var strength = PasswordStrengthCalculator.GetStrength(entropy);

        return Ok(new GenerateResult
        {
            Password = password,
            Entropy = entropy,
            Strength = strength
        });
    }
}
