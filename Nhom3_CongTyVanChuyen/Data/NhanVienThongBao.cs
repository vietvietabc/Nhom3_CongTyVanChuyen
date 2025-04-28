using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class NhanVienThongBao
    {
        public string MaNhanVien { get; set; }
        public string MaThongBao { get; set; }

        [ForeignKey("MaNhanVien")]
        public NhanVien NhanVien { get; set; }

        [ForeignKey("MaThongBao")]
        public ThongBao ThongBao { get; set; }
    }
}
