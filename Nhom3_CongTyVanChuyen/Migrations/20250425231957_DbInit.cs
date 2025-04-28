using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhom3_CongTyVanChuyen.Migrations
{
    /// <inheritdoc />
    public partial class DbInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    MaDanhMuc = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "TinhThanhPhos",
                columns: table => new
                {
                    MaTinhTP = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenTinhTP = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhThanhPhos", x => x.MaTinhTP);
                });

            migrationBuilder.CreateTable(
                name: "VaiTros",
                columns: table => new
                {
                    MaVaiTro = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenVaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTros", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "HangHoas",
                columns: table => new
                {
                    MaHangHoa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaDanhMuc = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenHangHoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonGia = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangHoas", x => x.MaHangHoa);
                    table.ForeignKey(
                        name: "FK_HangHoas_DanhMucs_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMucs",
                        principalColumn: "MaDanhMuc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuanHuyens",
                columns: table => new
                {
                    MaQuanHuyen = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaTinhTP = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenQuanHuyen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuanHuyens", x => x.MaQuanHuyen);
                    table.ForeignKey(
                        name: "FK_QuanHuyens_TinhThanhPhos_MaTinhTP",
                        column: x => x.MaTinhTP,
                        principalTable: "TinhThanhPhos",
                        principalColumn: "MaTinhTP",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhuongXas",
                columns: table => new
                {
                    MaPhuongXa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaQuanHuyen = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenPhuongXa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongXas", x => x.MaPhuongXa);
                    table.ForeignKey(
                        name: "FK_PhuongXas_QuanHuyens_MaQuanHuyen",
                        column: x => x.MaQuanHuyen,
                        principalTable: "QuanHuyens",
                        principalColumn: "MaQuanHuyen",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoNhas",
                columns: table => new
                {
                    MaSoNha = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaPhuongXa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DiaChiSoNha = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoNhas", x => x.MaSoNha);
                    table.ForeignKey(
                        name: "FK_SoNhas_PhuongXas_MaPhuongXa",
                        column: x => x.MaPhuongXa,
                        principalTable: "PhuongXas",
                        principalColumn: "MaPhuongXa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSoNha = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.MaKhachHang);
                    table.ForeignKey(
                        name: "FK_KhachHangs_SoNhas_MaSoNha",
                        column: x => x.MaSoNha,
                        principalTable: "SoNhas",
                        principalColumn: "MaSoNha",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaVaiTro = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaBuuCuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaSoNha = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.MaNhanVien);
                    table.ForeignKey(
                        name: "FK_NhanViens_SoNhas_MaSoNha",
                        column: x => x.MaSoNha,
                        principalTable: "SoNhas",
                        principalColumn: "MaSoNha",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhanViens_VaiTros_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTros",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NguoiNhans",
                columns: table => new
                {
                    MaNguoiNhan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSoNha = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiNhans", x => x.MaNguoiNhan);
                    table.ForeignKey(
                        name: "FK_NguoiNhans_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NguoiNhans_SoNhas_MaSoNha",
                        column: x => x.MaSoNha,
                        principalTable: "SoNhas",
                        principalColumn: "MaSoNha",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    MaDonHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaVanDon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TienDonHang = table.Column<double>(type: "float", nullable: false),
                    PhiGiaoHang = table.Column<double>(type: "float", nullable: false),
                    TienThuHo = table.Column<double>(type: "float", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiDonHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HanGioiTienThuHo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiThuHo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.MaDonHang);
                    table.ForeignKey(
                        name: "FK_DonHang_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHang_NhanViens_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    MaThongBao = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayThongBao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhanVienMaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBaos_NhanViens_NhanVienMaNhanVien",
                        column: x => x.NhanVienMaNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    MaChiTietDonHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaDonHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaHangHoa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KichThuoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TrongLuong = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.MaChiTietDonHang);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_HangHoas_MaHangHoa",
                        column: x => x.MaHangHoa,
                        principalTable: "HangHoas",
                        principalColumn: "MaHangHoa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanVienThongBaos",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaThongBao = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVienThongBaos", x => new { x.MaNhanVien, x.MaThongBao });
                    table.ForeignKey(
                        name: "FK_NhanVienThongBaos_NhanViens_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NhanVienThongBaos_ThongBaos_MaThongBao",
                        column: x => x.MaThongBao,
                        principalTable: "ThongBaos",
                        principalColumn: "MaThongBao",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaDonHang",
                table: "ChiTietDonHangs",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaHangHoa",
                table: "ChiTietDonHangs",
                column: "MaHangHoa");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaKhachHang",
                table: "DonHang",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaNhanVien",
                table: "DonHang",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoas_MaDanhMuc",
                table: "HangHoas",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_MaSoNha",
                table: "KhachHangs",
                column: "MaSoNha");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiNhans_MaKhachHang",
                table: "NguoiNhans",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiNhans_MaSoNha",
                table: "NguoiNhans",
                column: "MaSoNha");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaSoNha",
                table: "NhanViens",
                column: "MaSoNha");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaVaiTro",
                table: "NhanViens",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVienThongBaos_MaThongBao",
                table: "NhanVienThongBaos",
                column: "MaThongBao");

            migrationBuilder.CreateIndex(
                name: "IX_PhuongXas_MaQuanHuyen",
                table: "PhuongXas",
                column: "MaQuanHuyen");

            migrationBuilder.CreateIndex(
                name: "IX_QuanHuyens_MaTinhTP",
                table: "QuanHuyens",
                column: "MaTinhTP");

            migrationBuilder.CreateIndex(
                name: "IX_SoNhas_MaPhuongXa",
                table: "SoNhas",
                column: "MaPhuongXa");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_NhanVienMaNhanVien",
                table: "ThongBaos",
                column: "NhanVienMaNhanVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "NguoiNhans");

            migrationBuilder.DropTable(
                name: "NhanVienThongBaos");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "HangHoas");

            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "DanhMucs");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "SoNhas");

            migrationBuilder.DropTable(
                name: "VaiTros");

            migrationBuilder.DropTable(
                name: "PhuongXas");

            migrationBuilder.DropTable(
                name: "QuanHuyens");

            migrationBuilder.DropTable(
                name: "TinhThanhPhos");
        }
    }
}
