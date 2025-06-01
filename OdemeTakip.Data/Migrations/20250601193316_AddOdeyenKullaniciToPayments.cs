using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOdeyenKullaniciToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "SabitGiderler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "KrediTaksitler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "KrediKartiOdemeleri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "GenelOdemeler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "DegiskenOdemeSablonlari",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "DegiskenOdemeler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdeyenKullaniciAdi",
                table: "Cekler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "SabitGiderler");

            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "KrediTaksitler");

            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "KrediKartiOdemeleri");

            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "GenelOdemeler");

            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "DegiskenOdemeSablonlari");

            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "DegiskenOdemeler");

            migrationBuilder.DropColumn(
                name: "OdeyenKullaniciAdi",
                table: "Cekler");
        }
    }
}
