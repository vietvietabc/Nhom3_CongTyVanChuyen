namespace Nhom3_CongTyVanChuyen.Dtos
{
    // DTO cho cập nhật khách hàng
    public class KhachHangUpdateDto
    {
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public DateTime NgaySinh { get; set; }
        public string CCCD { get; set; }
        public string MaPhuongXa { get; set; }
        public string DiaChiSoNha { get; set; }
    }
}
