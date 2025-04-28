using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class KhachHang
    {
        [Key]
        public string MaKhachHang { get; set; }

        public string MaSoNha { get; set; }

        public string TenKhachHang { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public DateTime NgaySinh { get; set; }
        public string CCCD { get; set; }

        [ForeignKey("MaSoNha")]
        public SoNha SoNha { get; set; }

        public ICollection<DonHang> DonHangs { get; set; }
        public ICollection<NguoiNhan> NguoiNhans { get; set; }
    }
}