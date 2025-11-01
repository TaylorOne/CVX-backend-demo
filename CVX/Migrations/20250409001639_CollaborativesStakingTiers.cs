using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class CollaborativesStakingTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserExperience_Experience_SectorsId",
                table: "ApplicationUserExperience");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Experience",
                table: "Experience");

            migrationBuilder.RenameTable(
                name: "Experience",
                newName: "Sectors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sectors",
                table: "Sectors",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Collaboratives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCollaborative = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevenueShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IndirectCosts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollabLeaderCompensation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayoutFrequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collaboratives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollaborativeExperience",
                columns: table => new
                {
                    CollaborativesId = table.Column<int>(type: "int", nullable: false),
                    SectorsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaborativeExperience", x => new { x.CollaborativesId, x.SectorsId });
                    table.ForeignKey(
                        name: "FK_CollaborativeExperience_Collaboratives_CollaborativesId",
                        column: x => x.CollaborativesId,
                        principalTable: "Collaboratives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaborativeExperience_Sectors_SectorsId",
                        column: x => x.SectorsId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollaborativeSkills",
                columns: table => new
                {
                    CollaborativesId = table.Column<int>(type: "int", nullable: false),
                    SkillsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollaborativeSkills", x => new { x.CollaborativesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_CollaborativeSkills_Collaboratives_CollaborativesId",
                        column: x => x.CollaborativesId,
                        principalTable: "Collaboratives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollaborativeSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakingTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollaborativeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakingTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakingTiers_Collaboratives_CollaborativeId",
                        column: x => x.CollaborativeId,
                        principalTable: "Collaboratives",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollaborativeExperience_SectorsId",
                table: "CollaborativeExperience",
                column: "SectorsId");

            migrationBuilder.CreateIndex(
                name: "IX_CollaborativeSkills_SkillsId",
                table: "CollaborativeSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_StakingTiers_CollaborativeId",
                table: "StakingTiers",
                column: "CollaborativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserExperience_Sectors_SectorsId",
                table: "ApplicationUserExperience",
                column: "SectorsId",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserExperience_Sectors_SectorsId",
                table: "ApplicationUserExperience");

            migrationBuilder.DropTable(
                name: "CollaborativeExperience");

            migrationBuilder.DropTable(
                name: "CollaborativeSkills");

            migrationBuilder.DropTable(
                name: "StakingTiers");

            migrationBuilder.DropTable(
                name: "Collaboratives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sectors",
                table: "Sectors");

            migrationBuilder.RenameTable(
                name: "Sectors",
                newName: "Experience");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Experience",
                table: "Experience",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserExperience_Experience_SectorsId",
                table: "ApplicationUserExperience",
                column: "SectorsId",
                principalTable: "Experience",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
