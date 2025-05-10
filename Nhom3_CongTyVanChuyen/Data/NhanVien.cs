// File: Data/NhanVien.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class NhanVien
    {
        [Key]
        public string MaNhanVien { get; set; } = Guid.NewGuid().ToString();
        public string MaVaiTro { get; set; }
        public string TenNhanVien { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string MaSoNha { get; set; }
        public string CCCD { get; set; }

        // Thêm các trường audit
        public string NguoiTao { get; set; }
        public DateTime NgayTao { get; set; }
        public string NguoiCapNhat { get; set; }
        public DateTime? NgayCapNhat { get; set; }

        [ForeignKey("MaVaiTro")]
        public VaiTro VaiTro { get; set; }

        [ForeignKey("MaSoNha")]
        public SoNha SoNha { get; set; }

        public ICollection<DonHang> DonHangs { get; set; }
    }
}