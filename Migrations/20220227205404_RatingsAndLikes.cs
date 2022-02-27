using Microsoft.EntityFrameworkCore.Migrations;

namespace RazorCoursework.Migrations
{
    public partial class RatingsAndLikes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewLikes",
                columns: table => new
                {
                    LikeID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReviewID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLikes", x => x.LikeID);
                    table.ForeignKey(
                        name: "FK_ReviewLikes_Reviews_ReviewID",
                        column: x => x.ReviewID,
                        principalTable: "Reviews",
                        principalColumn: "ReviewID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewRatings",
                columns: table => new
                {
                    RatingID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReviewID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatingValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewRatings", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_ReviewRatings_Reviews_ReviewID",
                        column: x => x.ReviewID,
                        principalTable: "Reviews",
                        principalColumn: "ReviewID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLikes_ReviewID",
                table: "ReviewLikes",
                column: "ReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRatings_ReviewID",
                table: "ReviewRatings",
                column: "ReviewID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewLikes");

            migrationBuilder.DropTable(
                name: "ReviewRatings");
        }
    }
}
