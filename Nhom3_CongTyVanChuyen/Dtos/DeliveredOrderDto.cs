namespace Nhom3_CongTyVanChuyen.Dtos
{
    public class DeliveredOrderDto
    {
        public string MaDonHang { get; set; }
        public string TenDonHang { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public double TienThuHo { get; set; }
        public DateTime? NgayNhan { get; set; }
        public string TrangThaiThuHo { get; set; }
    }
}
