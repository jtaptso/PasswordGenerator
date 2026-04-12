using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordGenerator.Domain.Entities;

namespace PasswordGenerator.Infrastructure.Data.Configurations;

public class PasswordEntryConfiguration : IEntityTypeConfiguration<PasswordEntry>
{
    public void Configure(EntityTypeBuilder<PasswordEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.EncryptedPassword)
            .IsRequired();

        builder.Property(e => e.Website)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime2");
    }
}
