using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBib.Data.Migrations
{
    /// <inheritdoc />
    public partial class milagre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContaAtiva",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContaBloqueada",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RazaoBloqueio",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Livros",
                columns: table => new
                {
                    Id_Livros = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ISBN_Livros = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titulo_Livros = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Autor_Livros = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Genero_Livros = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataPublicacao_Livros = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Disponivel_Livros = table.Column<bool>(type: "bit", nullable: false),
                    Imagem_Livros = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livros", x => x.Id_Livros);
                });

            migrationBuilder.CreateTable(
                name: "LivrosRequerimentos",
                columns: table => new
                {
                    Id_LivrosRequerimento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LivrosId = table.Column<int>(type: "int", nullable: false),
                    Aprovado_LivrosRequerimento = table.Column<bool>(type: "bit", nullable: false),
                    Devolvido_LivrosRequerimento = table.Column<bool>(type: "bit", nullable: false),
                    AceitoPor_LivrosRequerimento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecusadoPor_LivrosRequerimento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataAprovacao_LivrosRequerimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao_LivrosRequerimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataRequerimento_LivrosRequerimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPrevisaoDevolucao_LivrosRequerimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TituloLivro_LivrosRequerimento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomeUtilizador_LivrosRequerimento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recusado_LivrosRequerimento = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivrosRequerimentos", x => x.Id_LivrosRequerimento);
                });

            migrationBuilder.CreateTable(
                name: "Perfils",
                columns: table => new
                {
                    Id_Perfil = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataNascimento_Perfil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeUtilizador_Perfil = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfils", x => x.Id_Perfil);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Livros");

            migrationBuilder.DropTable(
                name: "LivrosRequerimentos");

            migrationBuilder.DropTable(
                name: "Perfils");

            migrationBuilder.DropColumn(
                name: "ContaAtiva",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContaBloqueada",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RazaoBloqueio",
                table: "AspNetUsers");
        }
    }
}
