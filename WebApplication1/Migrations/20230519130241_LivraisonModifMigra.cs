using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class LivraisonModifMigra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Livraison_AspNetUsers_UserId",
                table: "Livraison");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Livraison",
                newName: "ClientLivraisonId");

            migrationBuilder.RenameIndex(
                name: "IX_Livraison_UserId",
                table: "Livraison",
                newName: "IX_Livraison_ClientLivraisonId");

            migrationBuilder.AddColumn<int>(
                name: "CamionLivraisonID",
                table: "Livraison",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChauffeurLivraisonId",
                table: "Livraison",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Livraison_CamionLivraisonID",
                table: "Livraison",
                column: "CamionLivraisonID");

            migrationBuilder.CreateIndex(
                name: "IX_Livraison_ChauffeurLivraisonId",
                table: "Livraison",
                column: "ChauffeurLivraisonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Livraison_AspNetUsers_ChauffeurLivraisonId",
                table: "Livraison",
                column: "ChauffeurLivraisonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Livraison_AspNetUsers_ClientLivraisonId",
                table: "Livraison",
                column: "ClientLivraisonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Livraison_Camions_CamionLivraisonID",
                table: "Livraison",
                column: "CamionLivraisonID",
                principalTable: "Camions",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Livraison_AspNetUsers_ChauffeurLivraisonId",
                table: "Livraison");

            migrationBuilder.DropForeignKey(
                name: "FK_Livraison_AspNetUsers_ClientLivraisonId",
                table: "Livraison");

            migrationBuilder.DropForeignKey(
                name: "FK_Livraison_Camions_CamionLivraisonID",
                table: "Livraison");

            migrationBuilder.DropIndex(
                name: "IX_Livraison_CamionLivraisonID",
                table: "Livraison");

            migrationBuilder.DropIndex(
                name: "IX_Livraison_ChauffeurLivraisonId",
                table: "Livraison");

            migrationBuilder.DropColumn(
                name: "CamionLivraisonID",
                table: "Livraison");

            migrationBuilder.DropColumn(
                name: "ChauffeurLivraisonId",
                table: "Livraison");

            migrationBuilder.RenameColumn(
                name: "ClientLivraisonId",
                table: "Livraison",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Livraison_ClientLivraisonId",
                table: "Livraison",
                newName: "IX_Livraison_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Livraison_AspNetUsers_UserId",
                table: "Livraison",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
