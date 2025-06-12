using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class xy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SirketAdi",
                table: "Krediler");

            migrationBuilder.AddColumn<int>(
                name: "CariFirmaId",
                table: "Krediler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Krediler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Cekler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Krediler_CariFirmaId",
                table: "Krediler",
                column: "CariFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Krediler_CompanyId",
                table: "Krediler",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Krediler_CariFirmalar_CariFirmaId",
                table: "Krediler",
                column: "CariFirmaId",
                principalTable: "CariFirmalar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Krediler_Companies_CompanyId",
                table: "Krediler",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Krediler_CariFirmalar_CariFirmaId",
                table: "Krediler");

            migrationBuilder.DropForeignKey(
                name: "FK_Krediler_Companies_CompanyId",
                table: "Krediler");

            migrationBuilder.DropIndex(
                name: "IX_Krediler_CariFirmaId",
                table: "Krediler");

            migrationBuilder.DropIndex(
                name: "IX_Krediler_CompanyId",
                table: "Krediler");

            migrationBuilder.DropColumn(
                name: "CariFirmaId",
                table: "Krediler");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Krediler");

            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Cekler");

            migrationBuilder.AddColumn<string>(
                name: "SirketAdi",
                table: "Krediler",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
