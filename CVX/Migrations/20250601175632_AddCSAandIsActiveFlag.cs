using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class AddCSAandIsActiveFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentCSAId",
                table: "Collaboratives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CSAAcceptedAt",
                table: "CollaborativeMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CSAAcceptedId",
                table: "CollaborativeMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CSAAcceptedStatus",
                table: "CollaborativeMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CollaborativeMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CSAs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollaborativeId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CSAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CSAs_Collaboratives_CollaborativeId",
                        column: x => x.CollaborativeId,
                        principalTable: "Collaboratives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collaboratives_CurrentCSAId",
                table: "Collaboratives",
                column: "CurrentCSAId",
                unique: true,
                filter: "[CurrentCSAId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CSAs_CollaborativeId",
                table: "CSAs",
                column: "CollaborativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collaboratives_CSAs_CurrentCSAId",
                table: "Collaboratives",
                column: "CurrentCSAId",
                principalTable: "CSAs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collaboratives_CSAs_CurrentCSAId",
                table: "Collaboratives");

            migrationBuilder.DropTable(
                name: "CSAs");

            migrationBuilder.DropIndex(
                name: "IX_Collaboratives_CurrentCSAId",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "CurrentCSAId",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "CSAAcceptedAt",
                table: "CollaborativeMembers");

            migrationBuilder.DropColumn(
                name: "CSAAcceptedId",
                table: "CollaborativeMembers");

            migrationBuilder.DropColumn(
                name: "CSAAcceptedStatus",
                table: "CollaborativeMembers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CollaborativeMembers");
        }
    }
}
