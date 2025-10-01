using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM1.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToVehicleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "VehicleModel",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "VehicleModel");
        }
    }
}
