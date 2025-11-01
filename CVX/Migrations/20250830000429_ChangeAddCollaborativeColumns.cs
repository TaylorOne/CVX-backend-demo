using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAddCollaborativeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CollabLeaderCompensation",
                table: "Collaboratives",
                newName: "CollabAdminCompensationPercent");

            migrationBuilder.AddColumn<decimal>(
                name: "LaunchTokensPriorWorkPercent",
                table: "Collaboratives",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaunchTokensPriorWorkPercent",
                table: "Collaboratives");

            migrationBuilder.RenameColumn(
                name: "CollabAdminCompensationPercent",
                table: "Collaboratives",
                newName: "CollabLeaderCompensation");
        }
    }
}
