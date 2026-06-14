using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minas_valor_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "BankAccounts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_OwnerId",
                table: "BankAccounts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Users_OwnerId",
                table: "BankAccounts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Users_OwnerId",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_OwnerId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "BankAccounts");
        }
    }
}
