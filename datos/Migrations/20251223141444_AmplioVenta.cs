using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class AmplioVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Efectivo",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Importe",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Ventas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Importe",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Ventas");

            migrationBuilder.AlterColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
