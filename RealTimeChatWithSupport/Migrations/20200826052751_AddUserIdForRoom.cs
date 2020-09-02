using Microsoft.EntityFrameworkCore.Migrations;

namespace RealTimeChatWithSupport.Migrations
{
    public partial class AddUserIdForRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ChatRooms",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ChatRooms");
        }
    }
}
