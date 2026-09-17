using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LenguajesFormalesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEncodingFacialToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "encoding_facial",
                table: "usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "encoding_facial",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "encoding_facial",
                table: "usuarios");
        }
    }
}
