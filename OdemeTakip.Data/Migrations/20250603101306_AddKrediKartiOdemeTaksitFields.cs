using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKrediKartiOdemeTaksitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IlkOdemeTarihi",
                table: "KrediKartiOdemeleri",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaksitNo",
                table: "KrediKartiOdemeleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToplamTaksit",
                table: "KrediKartiOdemeleri",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IlkOdemeTarihi",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropColumn(
                name: "TaksitNo",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropColumn(
                name: "ToplamTaksit",
                table: "KrediKartiOdemeleri");
        }
    }
}
