using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCajaYConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CajasSesiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoFinal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstaAbierta = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajasSesiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuraciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImprimirTicket = table.Column<bool>(type: "bit", nullable: false),
                    UsarControlCaja = table.Column<bool>(type: "bit", nullable: false),
                    NombreNegocio = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CajasSesiones");

            migrationBuilder.DropTable(
                name: "Configuraciones");
        }
    }
}
