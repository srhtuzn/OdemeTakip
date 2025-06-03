using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdemeTakip.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCariFirmaToOdemeler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CariFirmaId",
                table: "SabitGiderler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CariFirmaId",
                table: "GenelOdemeler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CariFirmaId",
                table: "DegiskenOdemeler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SabitGiderler_CariFirmaId",
                table: "SabitGiderler",
                column: "CariFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_GenelOdemeler_CariFirmaId",
                table: "GenelOdemeler",
                column: "CariFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_DegiskenOdemeler_CariFirmaId",
                table: "DegiskenOdemeler",
                column: "CariFirmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DegiskenOdemeler_CariFirmalar_CariFirmaId",
                table: "DegiskenOdemeler",
                column: "CariFirmaId",
                principalTable: "CariFirmalar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GenelOdemeler_CariFirmalar_CariFirmaId",
                table: "GenelOdemeler",
                column: "CariFirmaId",
                principalTable: "CariFirmalar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SabitGiderler_CariFirmalar_CariFirmaId",
                table: "SabitGiderler",
                column: "CariFirmaId",
                principalTable: "CariFirmalar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DegiskenOdemeler_CariFirmalar_CariFirmaId",
                table: "DegiskenOdemeler");

            migrationBuilder.DropForeignKey(
                name: "FK_GenelOdemeler_CariFirmalar_CariFirmaId",
                table: "GenelOdemeler");

            migrationBuilder.DropForeignKey(
                name: "FK_SabitGiderler_CariFirmalar_CariFirmaId",
                table: "SabitGiderler");

            migrationBuilder.DropIndex(
                name: "IX_SabitGiderler_CariFirmaId",
                table: "SabitGiderler");

            migrationBuilder.DropIndex(
                name: "IX_GenelOdemeler_CariFirmaId",
                table: "GenelOdemeler");

            migrationBuilder.DropIndex(
                name: "IX_DegiskenOdemeler_CariFirmaId",
                table: "DegiskenOdemeler");

            migrationBuilder.DropColumn(
                name: "CariFirmaId",
                table: "SabitGiderler");

            migrationBuilder.DropColumn(
                name: "CariFirmaId",
                table: "GenelOdemeler");

            migrationBuilder.DropColumn(
                name: "CariFirmaId",
                table: "DegiskenOdemeler");
        }
    }
}
