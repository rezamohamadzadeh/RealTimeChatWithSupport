using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RealTimeChatWithSupport.Migrations
{
    public partial class QuestionModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "ChatMessages",
                newName: "DateTime");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateTime",
                table: "ChatRooms",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "ChatRooms");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "ChatMessages",
                newName: "SentAt");
        }
    }
}
