using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RealTimeChatWithSupport.Migrations
{
    public partial class FormId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FormId",
                table: "QuestionAnswers",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormId",
                table: "QuestionAnswers");
        }
    }
}
