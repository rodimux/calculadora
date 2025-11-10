using CalculadoraCostes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CalculadoraCostes.Infrastructure.Persistence.Configurations;

public class SystemParameterConfiguration : IEntityTypeConfiguration<SystemParameter>
{
    public void Configure(EntityTypeBuilder<SystemParameter> builder)
    {
        builder.ToTable("SystemParameters");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.Key)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Category)
            .HasConversion<int>();

        builder.Property(p => p.Value)
            .HasColumnType("decimal(18,6)");

        builder.Property(p => p.Unit)
            .HasMaxLength(50);

        builder.Property(p => p.MinValue)
            .HasColumnType("decimal(18,6)");

        builder.Property(p => p.MaxValue)
            .HasColumnType("decimal(18,6)");

        builder.Property(p => p.IsEditable)
            .HasDefaultValue(true);
    }
}
