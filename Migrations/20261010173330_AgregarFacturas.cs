using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FerreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFacturas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Iva = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacturaLinea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FacturaId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    PorcentajeIva = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Iva = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaLinea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaLinea_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturaLinea_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaLinea_FacturaId",
                table: "FacturaLinea",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaLinea_ProductoId",
                table: "FacturaLinea",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturaLinea");

            migrationBuilder.DropTable(
                name: "Facturas");
        }
    }
}
