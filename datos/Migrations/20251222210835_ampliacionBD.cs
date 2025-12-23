using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Datos.Migrations
{
    /// <inheritdoc />
    public partial class ampliacionBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "Productos",
                newName: "PrecioVenta");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoBarras",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ControlarStock",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCompra",
                table: "Productos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TieneIVA",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnidadMedidaId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cuit",
                table: "Configuraciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Configuraciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ManejarIVA",
                table: "Configuraciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeIVA",
                table: "Configuraciones",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Abreviatura = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[] { 1, true, "General" });

            migrationBuilder.InsertData(
                table: "Configuraciones",
                columns: new[] { "Id", "Cuit", "Direccion", "ImprimirTicket", "ManejarIVA", "NombreNegocio", "PorcentajeIVA", "UsarControlCaja" },
                values: new object[] { 1, "00-00000000-0", "Sin Dirección Registrada", true, false, "Mi Negocio", 21m, true });

            migrationBuilder.InsertData(
                table: "UnidadesMedida",
                columns: new[] { "Id", "Abreviatura", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, "un", true, "Unidad" },
                    { 2, "kg", true, "Kilogramo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_UnidadMedidaId",
                table: "Productos",
                column: "UnidadMedidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_UnidadesMedida_UnidadMedidaId",
                table: "Productos",
                column: "UnidadMedidaId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_UnidadesMedida_UnidadMedidaId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");

            migrationBuilder.DropIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_UnidadMedidaId",
                table: "Productos");

            migrationBuilder.DeleteData(
                table: "Configuraciones",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CodigoBarras",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ControlarStock",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioCompra",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TieneIVA",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Cuit",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "ManejarIVA",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "PorcentajeIVA",
                table: "Configuraciones");

            migrationBuilder.RenameColumn(
                name: "PrecioVenta",
                table: "Productos",
                newName: "Precio");
        }
    }
}
