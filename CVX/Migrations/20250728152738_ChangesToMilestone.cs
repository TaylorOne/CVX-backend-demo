using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestones_ProjectMembers_ProjectMemberId",
                table: "Milestones");

            migrationBuilder.DropIndex(
                name: "IX_Milestones_ProjectMemberId",
                table: "Milestones");

            migrationBuilder.RenameColumn(
                name: "ProjectMemberId",
                table: "Milestones",
                newName: "InviteStatus");

            migrationBuilder.AddColumn<string>(
                name: "ArtifactUrl",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AssigneeId",
                table: "Milestones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Milestones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionSummary",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "Milestones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_AssigneeId",
                table: "Milestones",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestones_ProjectMembers_AssigneeId",
                table: "Milestones",
                column: "AssigneeId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestones_ProjectMembers_AssigneeId",
                table: "Milestones");

            migrationBuilder.DropIndex(
                name: "IX_Milestones_AssigneeId",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "ArtifactUrl",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "CompletionSummary",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "Milestones");

            migrationBuilder.RenameColumn(
                name: "InviteStatus",
                table: "Milestones",
                newName: "ProjectMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_ProjectMemberId",
                table: "Milestones",
                column: "ProjectMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestones_ProjectMembers_ProjectMemberId",
                table: "Milestones",
                column: "ProjectMemberId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
