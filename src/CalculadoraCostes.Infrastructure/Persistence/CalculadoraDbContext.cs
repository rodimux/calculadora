using CalculadoraCostes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraCostes.Infrastructure.Persistence;

public class CalculadoraDbContext(DbContextOptions<CalculadoraDbContext> options) : DbContext(options)
{
    public DbSet<Energy> Energies => Set<Energy>();

    public DbSet<EnergyCostComponent> EnergyCostComponents => Set<EnergyCostComponent>();

    public DbSet<SystemParameter> SystemParameters => Set<SystemParameter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalculadoraDbContext).Assembly);
    }
}
