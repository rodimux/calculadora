using CalculadoraCostes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CalculadoraCostes.Infrastructure.Persistence.Configurations;

public class EnergyConfiguration : IEntityTypeConfiguration<Energy>
{
    public void Configure(EntityTypeBuilder<Energy> builder)
    {
        builder.ToTable("Energies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(e => e.Family)
            .HasConversion<int>();

        builder.Property(e => e.Mode)
            .HasConversion<int>();

        builder.Property(e => e.PricePerUnit)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.ConsumptionPer100Km)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.RentingCostPerMonth)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.EmissionFactorPerUnit)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.RenewableShare)
            .HasColumnType("decimal(5,4)");

        builder.Property(e => e.EmissionReduction)
            .HasColumnType("decimal(5,4)");

        builder.HasMany(e => e.CostComponents)
            .WithOne(c => c.Energy)
            .HasForeignKey(c => c.EnergyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Energy>()
            .WithMany()
            .HasForeignKey(e => e.BaseEnergyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Energy>()
            .WithMany()
            .HasForeignKey(e => e.EmissionReferenceEnergyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
