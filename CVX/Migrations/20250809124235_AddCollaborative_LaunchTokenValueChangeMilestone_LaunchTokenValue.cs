using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class AddCollaborative_LaunchTokenValueChangeMilestone_LaunchTokenValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LaunchTokenValue",
                table: "Milestones",
                newName: "AllocatedLaunchTokens");

            migrationBuilder.RenameColumn(
                name: "FiatValue",
                table: "Milestones",
                newName: "AllocatedFiat");

            migrationBuilder.AddColumn<decimal>(
                name: "LaunchTokenValue",
                table: "Collaboratives",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaunchTokenValue",
                table: "Collaboratives");

            migrationBuilder.RenameColumn(
                name: "AllocatedLaunchTokens",
                table: "Milestones",
                newName: "LaunchTokenValue");

            migrationBuilder.RenameColumn(
                name: "AllocatedFiat",
                table: "Milestones",
                newName: "FiatValue");
        }
    }
}
