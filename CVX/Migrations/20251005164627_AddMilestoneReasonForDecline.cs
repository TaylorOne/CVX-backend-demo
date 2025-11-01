using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class AddMilestoneReasonForDecline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonForDecline",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonForDecline",
                table: "Milestones");
        }
    }
}
