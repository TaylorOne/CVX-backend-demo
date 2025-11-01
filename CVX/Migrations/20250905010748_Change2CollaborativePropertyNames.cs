using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class Change2CollaborativePropertyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeeksTillInitialTokenRelease",
                table: "Collaboratives",
                newName: "WeeksTillSecondTokenRelease");

            migrationBuilder.RenameColumn(
                name: "TokenInitialReleaseDate",
                table: "Collaboratives",
                newName: "SecondTokenReleaseDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeeksTillSecondTokenRelease",
                table: "Collaboratives",
                newName: "WeeksTillInitialTokenRelease");

            migrationBuilder.RenameColumn(
                name: "SecondTokenReleaseDate",
                table: "Collaboratives",
                newName: "TokenInitialReleaseDate");
        }
    }
}
