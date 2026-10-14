using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoLuki.Infrastructure.Persistence.Migrations
{
    public partial class RemoveReservedStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "Cars" SET "Status" = 'Available' WHERE "Status" = 'Reserved';""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
