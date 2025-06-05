using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKrediKartiNavigationToKrediKartiOdeme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KrediKartiId",
                table: "KrediKartiOdemeleri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KrediKartiOdemeleri_KrediKartiId",
                table: "KrediKartiOdemeleri",
                column: "KrediKartiId");

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartlari_KrediKartiId",
                table: "KrediKartiOdemeleri",
                column: "KrediKartiId",
                principalTable: "KrediKartlari",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartlari_KrediKartiId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropIndex(
                name: "IX_KrediKartiOdemeleri_KrediKartiId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropColumn(
                name: "KrediKartiId",
                table: "KrediKartiOdemeleri");
        }
    }
}
