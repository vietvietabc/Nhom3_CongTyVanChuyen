using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public string MaDonHang { get; set; }

        public string MaVanDon { get; set; }

        public string MaKhachHang { get; set; }
        public string MaNhanVien { get; set; }

        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; }

        [ForeignKey("MaNhanVien")]
        public NhanVien NhanVien { get; set; }

        public double TienDonHang { get; set; }
        public double PhiGiaoHang { get; set; }
        public double TienThuHo { get; set; }
        public DateTime NgayGui { get; set; }
        public DateTime NgayNhan { get; set; }
        public string TrangThaiDonHang { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public DateTime HanGioiTienThuHo { get; set; }
        public string TrangThaiThuHo { get; set; }
        public string GhiChu { get; set; }

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
