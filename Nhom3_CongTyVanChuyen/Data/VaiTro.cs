using System.ComponentModel.DataAnnotations;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class VaiTro
    {
        [Key]
        public string MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }

        public ICollection<NhanVien> NhanViens { get; set; }
    }
}
