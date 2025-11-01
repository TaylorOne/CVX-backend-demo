using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class AddLaunchTokenTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaunchTokenTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    CollaborativeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaunchTokenTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaunchTokenTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LaunchTokenTransactions_Collaboratives_CollaborativeId",
                        column: x => x.CollaborativeId,
                        principalTable: "Collaboratives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LaunchTokenTransactions_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LaunchTokenTransactions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaunchTokenTransactions_CollaborativeId",
                table: "LaunchTokenTransactions",
                column: "CollaborativeId");

            migrationBuilder.CreateIndex(
                name: "IX_LaunchTokenTransactions_MilestoneId",
                table: "LaunchTokenTransactions",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_LaunchTokenTransactions_ProjectId",
                table: "LaunchTokenTransactions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LaunchTokenTransactions_UserId",
                table: "LaunchTokenTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaunchTokenTransactions");
        }
    }
}
