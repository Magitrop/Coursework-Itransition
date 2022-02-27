using Microsoft.EntityFrameworkCore.Migrations;

namespace RazorCoursework.Migrations
{
    public partial class AddedUserReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReviewCreatorID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachedPictureLinks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewSubjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewSubjectGenre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewID);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagID);
                });

            migrationBuilder.CreateTable(
                name: "ReviewAndTagRelations",
                columns: table => new
                {
                    RelationID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReviewID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TagID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewAndTagRelations", x => x.RelationID);
                    table.ForeignKey(
                        name: "FK_ReviewAndTagRelations_Reviews_ReviewID",
                        column: x => x.ReviewID,
                        principalTable: "Reviews",
                        principalColumn: "ReviewID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewAndTagRelations_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "TagID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAndTagRelations_ReviewID",
                table: "ReviewAndTagRelations",
                column: "ReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAndTagRelations_TagID",
                table: "ReviewAndTagRelations",
                column: "TagID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewAndTagRelations");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
