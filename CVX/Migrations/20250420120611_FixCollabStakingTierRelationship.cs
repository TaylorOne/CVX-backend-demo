using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class FixCollabStakingTierRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StakingTiers_Collaboratives_CollaborativeId",
                table: "StakingTiers");

            migrationBuilder.AddForeignKey(
                name: "FK_StakingTiers_Collaboratives_CollaborativeId",
                table: "StakingTiers",
                column: "CollaborativeId",
                principalTable: "Collaboratives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StakingTiers_Collaboratives_CollaborativeId",
                table: "StakingTiers");

            migrationBuilder.AddForeignKey(
                name: "FK_StakingTiers_Collaboratives_CollaborativeId",
                table: "StakingTiers",
                column: "CollaborativeId",
                principalTable: "Collaboratives",
                principalColumn: "Id");
        }
    }
}
