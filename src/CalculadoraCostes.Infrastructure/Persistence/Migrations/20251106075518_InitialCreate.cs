using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalculadoraCostes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Energies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Family = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    BaseEnergyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmissionReferenceEnergyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ConsumptionPer100Km = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RentingCostPerMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmissionFactorPerUnit = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RenewableShare = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    EmissionReduction = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    InheritEmissionFromBase = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Energies_Energies_BaseEnergyId",
                        column: x => x.BaseEnergyId,
                        principalTable: "Energies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Energies_Energies_EmissionReferenceEnergyId",
                        column: x => x.EmissionReferenceEnergyId,
                        principalTable: "Energies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnergyCostComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnergyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyCostComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergyCostComponents_Energies_EnergyId",
                        column: x => x.EnergyId,
                        principalTable: "Energies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Energies_BaseEnergyId",
                table: "Energies",
                column: "BaseEnergyId");

            migrationBuilder.CreateIndex(
                name: "IX_Energies_Code",
                table: "Energies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Energies_EmissionReferenceEnergyId",
                table: "Energies",
                column: "EmissionReferenceEnergyId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyCostComponents_EnergyId_Key",
                table: "EnergyCostComponents",
                columns: new[] { "EnergyId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameters_Key",
                table: "SystemParameters",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnergyCostComponents");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropTable(
                name: "Energies");
        }
    }
}
