using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCariFirmaToOdemelerr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CariFirmaId",
                table: "DegiskenOdemeSablonlari",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DegiskenOdemeSablonlari_CariFirmaId",
                table: "DegiskenOdemeSablonlari",
                column: "CariFirmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DegiskenOdemeSablonlari_CariFirmalar_CariFirmaId",
                table: "DegiskenOdemeSablonlari",
                column: "CariFirmaId",
                principalTable: "CariFirmalar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DegiskenOdemeSablonlari_CariFirmalar_CariFirmaId",
                table: "DegiskenOdemeSablonlari");

            migrationBuilder.DropIndex(
                name: "IX_DegiskenOdemeSablonlari_CariFirmaId",
                table: "DegiskenOdemeSablonlari");

            migrationBuilder.DropColumn(
                name: "CariFirmaId",
                table: "DegiskenOdemeSablonlari");
        }
    }
}
