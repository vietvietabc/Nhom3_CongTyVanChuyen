using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom3_CongTyVanChuyen.Migrations
{
    /// <inheritdoc />
    public partial class nguoitao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "NhanViens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NguoiTao",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NguoiTao",
                table: "NhanViens");
        }
    }
}
