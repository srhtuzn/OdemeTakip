using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyToKrediKartis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "KrediKartlari");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "KrediKartiOdemeleri");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "KrediKartlari",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "KrediKartiOdemeleri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KrediKartlari_CompanyId",
                table: "KrediKartlari",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_KrediKartiOdemeleri_CompanyId",
                table: "KrediKartiOdemeleri",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiOdemeleri_Companies_CompanyId",
                table: "KrediKartiOdemeleri",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartlari_Companies_CompanyId",
                table: "KrediKartlari",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiOdemeleri_Companies_CompanyId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartlari_Companies_CompanyId",
                table: "KrediKartlari");

            migrationBuilder.DropIndex(
                name: "IX_KrediKartlari_CompanyId",
                table: "KrediKartlari");

            migrationBuilder.DropIndex(
                name: "IX_KrediKartiOdemeleri_CompanyId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "KrediKartlari");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "KrediKartlari",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "KrediKartiOdemeleri",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
