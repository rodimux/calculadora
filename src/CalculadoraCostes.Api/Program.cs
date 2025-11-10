using System.Text.Json;
using System.Text.Json.Serialization;
using CalculadoraCostes.Api.Mapping;
using CalculadoraCostes.Application.DependencyInjection;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Application.Models;
using CalculadoraCostes.Contracts;
using CalculadoraCostes.Contracts.Admin;
using CalculadoraCostes.Contracts.Calculator;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Domain.Enums;
using CalculadoraCostes.Infrastructure.DependencyInjection;
using CalculadoraCostes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.WriteIndented = false;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var environment = builder.Environment;
app.UseExceptionHandler();
if (!environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("default");

await EnsureDatabaseCreatedAsync(app.Services);

var api = app.MapGroup("/api");

api.MapPost("/calculator", async (CalculationRequestDto request, ICostCalculationService service, CancellationToken cancellationToken) =>
    {
        var scenario = new CalculationScenario(
            request.KmsPerDay ?? 0,
            request.DaysPerMonth ?? 0,
            request.TrailerType.ToDomain(),
            request.MarginOverride,
            request.PricePerTonCo2Override);

        var summary = await service.CalculateAsync(scenario, cancellationToken);
        return Results.Ok(summary.ToDto());
    })
    .WithName("CalculateEnergyCosts")
    .WithOpenApi();

var admin = api.MapGroup("/admin");

admin.MapGet("/energies", async (IEnergyRepository repository, CancellationToken cancellationToken) =>
    {
        var energies = await repository.GetAllAsync(cancellationToken);
        return Results.Ok(energies.Select(e => e.ToDto()));
    })
    .WithName("GetEnergies")
    .WithOpenApi();

admin.MapGet("/energies/{code}", async (string code, IEnergyRepository repository, CancellationToken cancellationToken) =>
    {
        var energy = await repository.GetByCodeAsync(code, cancellationToken);
        return energy is null ? Results.NotFound() : Results.Ok(energy.ToDto());
    })
    .WithName("GetEnergyByCode")
    .WithOpenApi();

admin.MapPost("/energies", async (EnergyDto dto, IEnergyRepository repository, CancellationToken cancellationToken) =>
    {
        var entity = dto.ToEntity();
        entity.CostComponents = dto.CostComponents.Select(c => c.ToEntity()).ToList();
        foreach (var component in entity.CostComponents)
        {
            component.EnergyId = entity.Id;
        }
        await repository.AddAsync(entity, cancellationToken);
        return Results.Created($"/api/admin/energies/{entity.Code}", entity.ToDto());
    })
    .WithName("CreateEnergy")
    .WithOpenApi();

admin.MapPut("/energies/{id:guid}", async (Guid id, EnergyDto dto, IEnergyRepository repository, CancellationToken cancellationToken) =>
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return Results.NotFound();
        }

        dto.ToEntity(existing);
        await repository.UpdateAsync(existing, cancellationToken);
        return Results.Ok(existing.ToDto());
    })
    .WithName("UpdateEnergy")
    .WithOpenApi();

admin.MapDelete("/energies/{id:guid}", async (Guid id, IEnergyRepository repository, CancellationToken cancellationToken) =>
    {
        await repository.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    })
    .WithName("DeleteEnergy")
    .WithOpenApi();

admin.MapPost("/energies/{energyId:guid}/components", async (Guid energyId, EnergyCostComponentDto dto, IEnergyCostComponentRepository repository, CancellationToken cancellationToken) =>
    {
        dto.EnergyId = energyId;
        var entity = dto.ToEntity();
        await repository.UpsertAsync(entity, cancellationToken);
        return Results.Ok(entity.ToDto());
    })
    .WithName("CreateEnergyComponent")
    .WithOpenApi();

admin.MapPut("/energies/{energyId:guid}/components/{componentId:guid}", async (Guid energyId, Guid componentId, EnergyCostComponentDto dto, IEnergyCostComponentRepository repository, CancellationToken cancellationToken) =>
    {
        dto.Id = componentId;
        dto.EnergyId = energyId;
        var entity = dto.ToEntity();
        await repository.UpsertAsync(entity, cancellationToken);
        return Results.Ok(entity.ToDto());
    })
    .WithName("UpdateEnergyComponent")
    .WithOpenApi();

admin.MapDelete("/energies/{energyId:guid}/components/{componentId:guid}", async (Guid componentId, IEnergyCostComponentRepository repository, CancellationToken cancellationToken) =>
    {
        await repository.DeleteAsync(componentId, cancellationToken);
        return Results.NoContent();
    })
    .WithName("DeleteEnergyComponent")
    .WithOpenApi();

admin.MapGet("/parameters", async (ISystemParameterRepository repository, CancellationToken cancellationToken) =>
    {
        var parameters = await repository.GetAllAsync(cancellationToken);
        return Results.Ok(parameters.Select(p => p.ToDto()));
    })
    .WithName("GetParameters")
    .WithOpenApi();

admin.MapPut("/parameters/{key}", async (string key, SystemParameterDto dto, ISystemParameterRepository repository, CancellationToken cancellationToken) =>
    {
        var entity = new SystemParameter
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Key = key,
            Name = dto.Name,
            Description = dto.Description,
            Category = Enum.Parse<ParameterCategory>(dto.Category, true),
            Unit = dto.Unit,
            Value = dto.Value,
            IsEditable = dto.IsEditable
        };

        await repository.UpsertAsync(entity, cancellationToken);
        return Results.Ok(entity.ToDto());
    })
    .WithName("UpdateParameter")
    .WithOpenApi();

admin.MapPost("/parameters/import", async (IParameterImportService importService, CancellationToken cancellationToken) =>
    {
        await importService.ImportFromSeedAsync(cancellationToken);
        return Results.Ok();
    })
    .WithName("ImportParameters")
    .WithOpenApi();

admin.MapPost("/energies/import", async (CalculadoraDbContext context, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
    {
        var logger = loggerFactory.CreateLogger("EnergyImport");
        await DataSeeder.SeedAsync(context, logger, cancellationToken, overwriteExisting: true);
        return Results.Ok();
    })
    .WithName("ImportEnergies")
    .WithOpenApi();

app.Run();

static async Task EnsureDatabaseCreatedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CalculadoraDbContext>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DataSeeder");
    await context.Database.MigrateAsync();
    await DataSeeder.SeedAsync(context, logger);
}
