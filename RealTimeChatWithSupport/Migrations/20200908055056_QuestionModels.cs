using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RealTimeChatWithSupport.Migrations
{
    public partial class QuestionModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateTime = table.Column<DateTimeOffset>(nullable: false),
                    FirstOption = table.Column<string>(nullable: false),
                    SecondOption = table.Column<string>(nullable: false),
                    ThirdOption = table.Column<string>(nullable: false),
                    FourthOption = table.Column<string>(nullable: false),
                    TrueAnswer = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateTime = table.Column<DateTimeOffset>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
