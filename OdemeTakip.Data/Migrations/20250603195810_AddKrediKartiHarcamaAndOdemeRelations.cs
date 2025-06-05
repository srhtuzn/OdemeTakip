using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKrediKartiHarcamaAndOdemeRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KrediKartiHarcama",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KrediKartiId = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaksitSayisi = table.Column<int>(type: "int", nullable: false),
                    HarcamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KrediKartiHarcama", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KrediKartiHarcama_KrediKartlari_KrediKartiId",
                        column: x => x.KrediKartiId,
                        principalTable: "KrediKartlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KrediKartiOdemeleri_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri",
                column: "KrediKartiHarcamaId");

            migrationBuilder.CreateIndex(
                name: "IX_KrediKartiHarcama_KrediKartiId",
                table: "KrediKartiHarcama",
                column: "KrediKartiId");

            migrationBuilder.AddForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartiHarcama_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri",
                column: "KrediKartiHarcamaId",
                principalTable: "KrediKartiHarcama",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KrediKartiOdemeleri_KrediKartiHarcama_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropTable(
                name: "KrediKartiHarcama");

            migrationBuilder.DropIndex(
                name: "IX_KrediKartiOdemeleri_KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropColumn(
                name: "KrediKartiHarcamaId",
                table: "KrediKartiOdemeleri");
        }
    }
}
