using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class LaunchTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTokenRelease",
                table: "Collaboratives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaunchCyclePeriod",
                table: "Collaboratives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LaunchTokensBalance",
                table: "Collaboratives",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LaunchTokensCreated",
                table: "Collaboratives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenInitialReleaseDate",
                table: "Collaboratives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TokenReleaseRate",
                table: "Collaboratives",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTokenRelease",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "LaunchCyclePeriod",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "LaunchTokensBalance",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "LaunchTokensCreated",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "TokenInitialReleaseDate",
                table: "Collaboratives");

            migrationBuilder.DropColumn(
                name: "TokenReleaseRate",
                table: "Collaboratives");
        }
    }
}
