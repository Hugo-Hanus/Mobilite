using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class MigraCamion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Livraison_Camions_CamionLivraisonID",
                table: "Livraison");

            migrationBuilder.AlterColumn<int>(
                name: "CamionLivraisonID",
                table: "Livraison",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Livraison_Camions_CamionLivraisonID",
                table: "Livraison",
                column: "CamionLivraisonID",
                principalTable: "Camions",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Livraison_Camions_CamionLivraisonID",
                table: "Livraison");

            migrationBuilder.AlterColumn<int>(
                name: "CamionLivraisonID",
                table: "Livraison",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Livraison_Camions_CamionLivraisonID",
                table: "Livraison",
                column: "CamionLivraisonID",
                principalTable: "Camions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
