using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class x : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SablonId",
                table: "DegiskenOdemeler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DegiskenOdemeler_SablonId",
                table: "DegiskenOdemeler",
                column: "SablonId");

            migrationBuilder.AddForeignKey(
                name: "FK_DegiskenOdemeler_DegiskenOdemeSablonlari_SablonId",
                table: "DegiskenOdemeler",
                column: "SablonId",
                principalTable: "DegiskenOdemeSablonlari",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DegiskenOdemeler_DegiskenOdemeSablonlari_SablonId",
                table: "DegiskenOdemeler");

            migrationBuilder.DropIndex(
                name: "IX_DegiskenOdemeler_SablonId",
                table: "DegiskenOdemeler");

            migrationBuilder.DropColumn(
                name: "SablonId",
                table: "DegiskenOdemeler");
        }
    }
}
