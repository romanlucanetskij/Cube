using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DripCube.Migrations
{

    public partial class AddPaymentStatus : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "Orders",
                type: "text",
                nullable: true);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "Orders");
        }
    }
}
