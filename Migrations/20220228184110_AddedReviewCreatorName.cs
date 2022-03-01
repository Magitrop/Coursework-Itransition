using Microsoft.EntityFrameworkCore.Migrations;

namespace RazorCoursework.Migrations
{
    public partial class AddedReviewCreatorName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewCreatorName",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewCreatorName",
                table: "Reviews");
        }
    }
}
