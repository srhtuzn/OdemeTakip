using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKrediKartiHarcamaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiHarcama_KrediKartlari_KrediKartiId",
                table: "KrediKartiHarcama");

            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartiHarcama_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KrediKartiHarcama",
                table: "KrediKartiHarcama");

            migrationBuilder.RenameTable(
                name: "KrediKartiHarcama",
                newName: "KrediKartiHarcamalari");

            migrationBuilder.RenameIndex(
                name: "IX_KrediKartiHarcama_KrediKartiId",
                table: "KrediKartiHarcamalari",
                newName: "IX_KrediKartiHarcamalari_KrediKartiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KrediKartiHarcamalari",
                table: "KrediKartiHarcamalari",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiHarcamalari_KrediKartlari_KrediKartiId",
                table: "KrediKartiHarcamalari",
                column: "KrediKartiId",
                principalTable: "KrediKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartiHarcamalari_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri",
                column: "KrediKartiHarcamaId",
                principalTable: "KrediKartiHarcamalari",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiHarcamalari_KrediKartlari_KrediKartiId",
                table: "KrediKartiHarcamalari");

            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartiHarcamalari_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KrediKartiHarcamalari",
                table: "KrediKartiHarcamalari");

            migrationBuilder.RenameTable(
                name: "KrediKartiHarcamalari",
                newName: "KrediKartiHarcama");

            migrationBuilder.RenameIndex(
                name: "IX_KrediKartiHarcamalari_KrediKartiId",
                table: "KrediKartiHarcama",
                newName: "IX_KrediKartiHarcama_KrediKartiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KrediKartiHarcama",
                table: "KrediKartiHarcama",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiHarcama_KrediKartlari_KrediKartiId",
                table: "KrediKartiHarcama",
                column: "KrediKartiId",
                principalTable: "KrediKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartiHarcama_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri",
                column: "KrediKartiHarcamaId",
                principalTable: "KrediKartiHarcama",
                principalColumn: "Id");
        }
    }
}
