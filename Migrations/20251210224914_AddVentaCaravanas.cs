using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCaravana.Migrations
{
    /// <inheritdoc />
    public partial class AddVentaCaravanas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Caravanas_CaravanaId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_CaravanaId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CaravanaId",
                table: "Ventas");

            migrationBuilder.CreateTable(
                name: "VentaCaravanas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CaravanaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaCaravanas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaCaravanas_Caravanas_CaravanaId",
                        column: x => x.CaravanaId,
                        principalTable: "Caravanas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaCaravanas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaCaravanas_CaravanaId",
                table: "VentaCaravanas",
                column: "CaravanaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaCaravanas_VentaId",
                table: "VentaCaravanas",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentaCaravanas");

            migrationBuilder.AddColumn<int>(
                name: "CaravanaId",
                table: "Ventas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CaravanaId",
                table: "Ventas",
                column: "CaravanaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Caravanas_CaravanaId",
                table: "Ventas",
                column: "CaravanaId",
                principalTable: "Caravanas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
