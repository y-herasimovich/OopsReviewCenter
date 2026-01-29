using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OopsReviewCenter.Migrations
{
    /// <inheritdoc />
    public partial class RemoveResolvedByUserIdFromIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Users_ResolvedByUserId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_ResolvedByUserId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "Incidents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResolvedByUserId",
                table: "Incidents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ResolvedByUserId",
                table: "Incidents",
                column: "ResolvedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_ResolvedByUserId",
                table: "Incidents",
                column: "ResolvedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
