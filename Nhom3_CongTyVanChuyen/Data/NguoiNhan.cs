using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class NguoiNhan
    {
        [Key]
        public string MaNguoiNhan { get; set; }

        public string MaKhachHang { get; set; }
        public string MaSoNha { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }

        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; }

        [ForeignKey("MaSoNha")]
        public SoNha SoNha { get; set; }
    }
}
