using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedlockDeneme.Migrations
{
    /// <inheritdoc />
    public partial class YeniDegisiklikler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sepets",
                columns: table => new
                {
                    SepetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunId = table.Column<int>(type: "int", nullable: false),
                    UrunAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adet = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sepets", x => x.SepetId);
                    table.ForeignKey(
                        name: "FK_Sepets_Stoks_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Stoks",
                        principalColumn: "StokId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sepets_UrunId",
                table: "Sepets",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sepets");
        }
    }
}
