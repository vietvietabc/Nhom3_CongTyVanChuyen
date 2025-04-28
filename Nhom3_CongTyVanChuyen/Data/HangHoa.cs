using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class HangHoa
    {
        [Key]
        public string MaHangHoa { get; set; }

        public string MaDanhMuc { get; set; }
        public string TenHangHoa { get; set; }
        public double DonGia { get; set; }

        [ForeignKey("MaDanhMuc")]
        public DanhMuc DanhMuc { get; set; }

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
