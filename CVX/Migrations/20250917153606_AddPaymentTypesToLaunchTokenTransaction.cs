using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTypesToLaunchTokenTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaunchTokenTransactions_Milestones_MilestoneId",
                table: "LaunchTokenTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LaunchTokenTransactions_Projects_ProjectId",
                table: "LaunchTokenTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "LaunchTokenTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MilestoneId",
                table: "LaunchTokenTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "LaunchTokenTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_LaunchTokenTransactions_Milestones_MilestoneId",
                table: "LaunchTokenTransactions",
                column: "MilestoneId",
                principalTable: "Milestones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LaunchTokenTransactions_Projects_ProjectId",
                table: "LaunchTokenTransactions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaunchTokenTransactions_Milestones_MilestoneId",
                table: "LaunchTokenTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LaunchTokenTransactions_Projects_ProjectId",
                table: "LaunchTokenTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "LaunchTokenTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "LaunchTokenTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MilestoneId",
                table: "LaunchTokenTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LaunchTokenTransactions_Milestones_MilestoneId",
                table: "LaunchTokenTransactions",
                column: "MilestoneId",
                principalTable: "Milestones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LaunchTokenTransactions_Projects_ProjectId",
                table: "LaunchTokenTransactions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
