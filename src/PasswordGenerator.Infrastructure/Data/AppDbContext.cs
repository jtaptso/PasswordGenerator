using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Domain.Entities;
using PasswordGenerator.Infrastructure.Data.Configurations;

namespace PasswordGenerator.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PasswordEntry> PasswordEntries => Set<PasswordEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PasswordEntryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
