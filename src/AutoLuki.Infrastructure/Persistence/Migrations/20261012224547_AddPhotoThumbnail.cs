using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoLuki.Infrastructure.Persistence.Migrations
{
    public partial class AddPhotoThumbnail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbBlobName",
                table: "CarPhotos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbBlobName",
                table: "CarPhotos");
        }
    }
}
