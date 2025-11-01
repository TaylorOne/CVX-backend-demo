using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class ProjectLaunchTokenBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LaunchTokenBalance",
                table: "Projects",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaunchTokenBalance",
                table: "Projects");
        }
    }
}
