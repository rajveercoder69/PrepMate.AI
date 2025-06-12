using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiCRUDOps.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableforLLM1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LearningPdfs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "LearningPdfs");
        }
    }
}
