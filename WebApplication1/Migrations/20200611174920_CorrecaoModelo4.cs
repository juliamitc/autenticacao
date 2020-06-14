using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication1.Migrations
{
    public partial class CorrecaoModelo4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioSolicitacao_Usuario_UsuarioId",
                table: "UsuarioSolicitacao");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "UsuarioSolicitacao",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioSolicitacao_Usuario_UsuarioId",
                table: "UsuarioSolicitacao",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioSolicitacao_Usuario_UsuarioId",
                table: "UsuarioSolicitacao");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "UsuarioSolicitacao",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioSolicitacao_Usuario_UsuarioId",
                table: "UsuarioSolicitacao",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
