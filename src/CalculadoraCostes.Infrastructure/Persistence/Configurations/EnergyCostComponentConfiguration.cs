using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CalculadoraCostes.Infrastructure.Persistence.Configurations;

public class EnergyCostComponentConfiguration : IEntityTypeConfiguration<EnergyCostComponent>
{
    public void Configure(EntityTypeBuilder<EnergyCostComponent> builder)
    {
        builder.ToTable("EnergyCostComponents");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Category)
            .HasConversion<int>();

        builder.Property(c => c.ValueType)
            .HasConversion<int>();

        builder.Property(c => c.Value)
            .HasColumnType("decimal(18,6)");

        builder.HasIndex(c => new { c.EnergyId, c.Key })
            .IsUnique();

        builder.Property(c => c.IsEditable)
            .HasDefaultValue(true);

        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        builder.Property(c => c.Order)
            .HasDefaultValue(0);
    }
}
