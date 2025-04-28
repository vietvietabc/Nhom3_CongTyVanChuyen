using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class ChiTietDonHang
    {
        [Key]
        public string MaChiTietDonHang { get; set; }

        public string MaDonHang { get; set; }
        public string MaHangHoa { get; set; }
        public string KichThuoc { get; set; }
        public int SoLuong { get; set; }
        public double TrongLuong { get; set; }

        [ForeignKey("MaDonHang")]
        public DonHang DonHang { get; set; }

        [ForeignKey("MaHangHoa")]
        public HangHoa HangHoa { get; set; }
    }
}
