using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApiCRUDOps.Migrations
{
    /// <inheritdoc />
    public partial class updatetabletyep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "personDetail",
                keyColumn: "personId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "personDetail",
                keyColumn: "personId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "personDetail",
                keyColumn: "personId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "personDetail",
                keyColumn: "personId",
                keyValue: 4);

            migrationBuilder.AlterColumn<string>(
                name: "firstName",
                table: "personDetail",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_personDetail_firstName",
                table: "personDetail",
                column: "firstName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_personDetail_firstName",
                table: "personDetail");

            migrationBuilder.AlterColumn<string>(
                name: "firstName",
                table: "personDetail",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
        }
    }
}
