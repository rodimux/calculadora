IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Energies] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Family] int NOT NULL,
    [Mode] int NOT NULL,
    [BaseEnergyId] uniqueidentifier NULL,
    [EmissionReferenceEnergyId] uniqueidentifier NULL,
    [PricePerUnit] decimal(18,4) NOT NULL,
    [ConsumptionPer100Km] decimal(18,4) NOT NULL,
    [RentingCostPerMonth] decimal(18,2) NOT NULL,
    [EmissionFactorPerUnit] decimal(18,6) NOT NULL,
    [RenewableShare] decimal(5,4) NULL,
    [EmissionReduction] decimal(5,4) NULL,
    [InheritEmissionFromBase] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Energies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Energies_Energies_BaseEnergyId] FOREIGN KEY ([BaseEnergyId]) REFERENCES [Energies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Energies_Energies_EmissionReferenceEnergyId] FOREIGN KEY ([EmissionReferenceEnergyId]) REFERENCES [Energies] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SystemParameters] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(100) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Category] int NOT NULL,
    [Value] decimal(18,6) NOT NULL,
    [Unit] nvarchar(50) NULL,
    [MinValue] decimal(18,6) NULL,
    [MaxValue] decimal(18,6) NULL,
    [IsEditable] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_SystemParameters] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [EnergyCostComponents] (
    [Id] uniqueidentifier NOT NULL,
    [EnergyId] uniqueidentifier NOT NULL,
    [Key] nvarchar(100) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Category] int NOT NULL,
    [ValueType] int NOT NULL,
    [Value] decimal(18,6) NOT NULL,
    [Order] int NOT NULL DEFAULT 0,
    [IsEditable] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Notes] nvarchar(500) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_EnergyCostComponents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EnergyCostComponents_Energies_EnergyId] FOREIGN KEY ([EnergyId]) REFERENCES [Energies] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Energies_BaseEnergyId] ON [Energies] ([BaseEnergyId]);
GO

CREATE UNIQUE INDEX [IX_Energies_Code] ON [Energies] ([Code]);
GO

CREATE INDEX [IX_Energies_EmissionReferenceEnergyId] ON [Energies] ([EmissionReferenceEnergyId]);
GO

CREATE UNIQUE INDEX [IX_EnergyCostComponents_EnergyId_Key] ON [EnergyCostComponents] ([EnergyId], [Key]);
GO

CREATE UNIQUE INDEX [IX_SystemParameters_Key] ON [SystemParameters] ([Key]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251106075518_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

