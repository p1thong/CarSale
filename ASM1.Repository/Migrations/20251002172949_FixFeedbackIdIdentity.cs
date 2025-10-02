using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM1.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixFeedbackIdIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Feedback",
                newName: "rating");

            migrationBuilder.RenameColumn(
                name: "FeedbackDate",
                table: "Feedback",
                newName: "feedbackDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "feedbackDate",
                table: "Feedback",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "rating",
                table: "Feedback",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "feedbackDate",
                table: "Feedback",
                newName: "FeedbackDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FeedbackDate",
                table: "Feedback",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);
        }
    }
}
