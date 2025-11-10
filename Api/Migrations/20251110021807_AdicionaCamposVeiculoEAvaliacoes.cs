using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mininal_api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCamposVeiculoEAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Veiculos",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Veiculos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Placa",
                table: "Veiculos",
                type: "varchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Preco",
                table: "Veiculos",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Quilometragem",
                table: "Veiculos",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Veiculos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AvaliacoesVeiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VeiculoId = table.Column<int>(type: "int", nullable: false),
                    AdministradorId = table.Column<int>(type: "int", nullable: false),
                    Estrelas = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAvaliacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesVeiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesVeiculos_Administradores_AdministradorId",
                        column: x => x.AdministradorId,
                        principalTable: "Administradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvaliacoesVeiculos_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VeiculoFotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VeiculoId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeiculoFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VeiculoFotos_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesVeiculos_AdministradorId",
                table: "AvaliacoesVeiculos",
                column: "AdministradorId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesVeiculos_VeiculoId",
                table: "AvaliacoesVeiculos",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_VeiculoFotos_VeiculoId",
                table: "VeiculoFotos",
                column: "VeiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvaliacoesVeiculos");

            migrationBuilder.DropTable(
                name: "VeiculoFotos");

            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Placa",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Preco",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Quilometragem",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Veiculos");
        }
    }
}
