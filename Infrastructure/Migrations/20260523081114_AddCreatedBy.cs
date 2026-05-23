using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Sessions",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Captures",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CreatedById",
                table: "Sessions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Captures_CreatedById",
                table: "Captures",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_CreatedById",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Captures_CreatedById",
                table: "Captures");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Captures");
        }
    }
}
