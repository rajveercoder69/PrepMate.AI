using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApiCRUDOps.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "personDetail",
                columns: table => new
                {
                    personId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    price = table.Column<double>(type: "float", nullable: false),
                    size = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personDetail", x => x.personId);
                });

            migrationBuilder.CreateTable(
                name: "PdfUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalPdf = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CompressedPdf = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PdfUploads_personDetail_PersonId",
                        column: x => x.PersonId,
                        principalTable: "personDetail",
                        principalColumn: "personId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "personDetail",
                columns: new[] { "personId", "firstName", "gender", "lastName", "price", "size" },
                values: new object[,]
                {
                    { 1, "blue", "men", "Arrow", 49.990000000000002, 28 },
                    { 2, "paleblue", "men", "PeterEngland", 119.98999999999999, 38 },
                    { 3, "marron", "men", "Jameshamstephedan", 49.990000000000002, 42 },
                    { 4, "BabyPinkishBlue", "women", "Zara", 149.99000000000001, 28 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PdfUploads_PersonId",
                table: "PdfUploads",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PdfUploads");

            migrationBuilder.DropTable(
                name: "personDetail");
        }
    }
}
