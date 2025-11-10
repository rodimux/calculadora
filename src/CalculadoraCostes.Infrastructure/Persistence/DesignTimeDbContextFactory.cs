using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CalculadoraCostes.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CalculadoraDbContext>
{
    public CalculadoraDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CalculadoraDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=CalculadoraCostes;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new CalculadoraDbContext(optionsBuilder.Options);
    }
}
