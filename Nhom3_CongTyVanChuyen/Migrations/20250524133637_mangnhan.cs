using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom3_CongTyVanChuyen.Migrations
{
    /// <inheritdoc />
    public partial class mangnhan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaNguoiNhan",
                table: "DonHang",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaNguoiNhan",
                table: "DonHang",
                column: "MaNguoiNhan");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHang_NguoiNhans_MaNguoiNhan",
                table: "DonHang",
                column: "MaNguoiNhan",
                principalTable: "NguoiNhans",
                principalColumn: "MaNguoiNhan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHang_NguoiNhans_MaNguoiNhan",
                table: "DonHang");

            migrationBuilder.DropIndex(
                name: "IX_DonHang_MaNguoiNhan",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "MaNguoiNhan",
                table: "DonHang");
        }
    }
}
