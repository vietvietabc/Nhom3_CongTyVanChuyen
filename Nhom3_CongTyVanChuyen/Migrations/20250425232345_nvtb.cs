using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom3_CongTyVanChuyen.Migrations
{
    /// <inheritdoc />
    public partial class nvtb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThongBaos_NhanViens_NhanVienMaNhanVien",
                table: "ThongBaos");

            migrationBuilder.DropIndex(
                name: "IX_ThongBaos_NhanVienMaNhanVien",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "NhanVienMaNhanVien",
                table: "ThongBaos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NhanVienMaNhanVien",
                table: "ThongBaos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_NhanVienMaNhanVien",
                table: "ThongBaos",
                column: "NhanVienMaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBaos_NhanViens_NhanVienMaNhanVien",
                table: "ThongBaos",
                column: "NhanVienMaNhanVien",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien");
        }
    }
}
