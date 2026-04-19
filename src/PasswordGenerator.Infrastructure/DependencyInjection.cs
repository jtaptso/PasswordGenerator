using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Application.Interfaces;
using PasswordGenerator.Application.Services;
using PasswordGenerator.Infrastructure.Data;
using PasswordGenerator.Infrastructure.Repositories;
using PasswordGenerator.Infrastructure.Services;

namespace PasswordGenerator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("PasswordGeneratorDb"));

        services.AddDataProtection();

        services.AddScoped<IVaultRepository, VaultRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
        services.AddScoped<VaultService>();
        services.AddScoped<AuthService>();

        return services;
    }
}
