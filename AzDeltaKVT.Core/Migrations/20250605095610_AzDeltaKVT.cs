using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzDeltaKVT.Core.Migrations
{
    /// <inheritdoc />
    public partial class AzDeltaKVT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Chromosome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Start = table.Column<int>(type: "int", nullable: false),
                    Stop = table.Column<int>(type: "int", nullable: false),
                    UserInfo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genes", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Variants",
                columns: table => new
                {
                    VariantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chromosome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Alternative = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserInfo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variants", x => x.VariantId);
                });

            migrationBuilder.CreateTable(
                name: "NmTranscripts",
                columns: table => new
                {
                    NmNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GeneId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsSelect = table.Column<bool>(type: "bit", nullable: false),
                    IsClinical = table.Column<bool>(type: "bit", nullable: false),
                    IsInHouse = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmTranscripts", x => x.NmNumber);
                    table.ForeignKey(
                        name: "FK_NmTranscripts_Genes_GeneId",
                        column: x => x.GeneId,
                        principalTable: "Genes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneVariants",
                columns: table => new
                {
                    NmId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    BiologicalEffect = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserInfo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneVariants", x => new { x.NmId, x.VariantId });
                    table.ForeignKey(
                        name: "FK_GeneVariants_NmTranscripts_NmId",
                        column: x => x.NmId,
                        principalTable: "NmTranscripts",
                        principalColumn: "NmNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneVariants_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "VariantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneVariants_VariantId",
                table: "GeneVariants",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_NmTranscripts_GeneId",
                table: "NmTranscripts",
                column: "GeneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneVariants");

            migrationBuilder.DropTable(
                name: "NmTranscripts");

            migrationBuilder.DropTable(
                name: "Variants");

            migrationBuilder.DropTable(
                name: "Genes");
        }
    }
}
