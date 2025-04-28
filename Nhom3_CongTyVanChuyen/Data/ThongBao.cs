using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class ThongBao
    {
        [Key]
        public string MaThongBao { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayThongBao { get; set; }

        public ICollection<NhanVienThongBao> NhanVienThongBaos { get; set; }
    }
}
