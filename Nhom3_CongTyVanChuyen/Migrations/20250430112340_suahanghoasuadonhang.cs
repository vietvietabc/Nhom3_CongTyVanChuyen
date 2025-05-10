using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom3_CongTyVanChuyen.Migrations
{
    /// <inheritdoc />
    public partial class suahanghoasuadonhang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TienDonHang",
                table: "DonHang");

            migrationBuilder.RenameColumn(
                name: "TenHangHoa",
                table: "HangHoas",
                newName: "TinhChatHangHoa");

            migrationBuilder.AddColumn<string>(
                name: "TenDonHang",
                table: "DonHang",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenDonHang",
                table: "DonHang");

            migrationBuilder.RenameColumn(
                name: "TinhChatHangHoa",
                table: "HangHoas",
                newName: "TenHangHoa");

            migrationBuilder.AddColumn<double>(
                name: "TienDonHang",
                table: "DonHang",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
