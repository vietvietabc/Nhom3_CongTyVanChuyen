using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom3_CongTyVanChuyen.Migrations
{
    /// <inheritdoc />
    public partial class xoamabuucuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaBuuCuc",
                table: "NhanViens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaBuuCuc",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
