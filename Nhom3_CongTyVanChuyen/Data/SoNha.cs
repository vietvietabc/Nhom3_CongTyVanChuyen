using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class SoNha
    {
        [Key]
        public string MaSoNha { get; set; }

        public string MaPhuongXa { get; set; }
        public string DiaChiSoNha { get; set; }

        [ForeignKey("MaPhuongXa")]
        public PhuongXa PhuongXa { get; set; }

        public ICollection<KhachHang> KhachHangs { get; set; }
        public ICollection<NguoiNhan> NguoiNhans { get; set; }
        public ICollection<NhanVien> NhanViens { get; set; }
    }
}
