using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVX.Migrations
{
    /// <inheritdoc />
    public partial class MakeLaunchTokenTransactionUserNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaunchTokenTransactions_AspNetUsers_UserId",
                table: "LaunchTokenTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LaunchTokenTransactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_LaunchTokenTransactions_AspNetUsers_UserId",
                table: "LaunchTokenTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaunchTokenTransactions_AspNetUsers_UserId",
                table: "LaunchTokenTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LaunchTokenTransactions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LaunchTokenTransactions_AspNetUsers_UserId",
                table: "LaunchTokenTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
