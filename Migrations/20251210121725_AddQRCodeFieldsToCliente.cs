using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCaravana.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeFieldsToCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoQR",
                table: "Clientes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagenQR",
                table: "Clientes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoQR",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RutaImagenQR",
                table: "Clientes");
        }
    }
}
