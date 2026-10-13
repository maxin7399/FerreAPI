using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVendedorFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Paso 1: crear la columna permitiendo nulos
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Facturas",
                type: "integer",
                nullable: true);

            // Paso 2: asignar las facturas existentes al primer administrador
            migrationBuilder.Sql("""
        UPDATE "Facturas"
        SET "UsuarioId" = (SELECT MIN("Id") FROM "Usuarios" WHERE "Rol" = 'Administrador');
        """);

            // Paso 3: hacerla obligatoria
            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Facturas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_UsuarioId",
                table: "Facturas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Usuarios_UsuarioId",
                table: "Facturas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Usuarios_UsuarioId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_UsuarioId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Facturas");
        }
    }
}
