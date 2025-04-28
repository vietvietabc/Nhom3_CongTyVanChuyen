namespace Nhom3_CongTyVanChuyen.DTOs.Requests
{
    public class NhanVienRequest
    {
        public string TenNhanVien { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public string SDT { get; set; }
        public string CCCD { get; set; }
        public string MaVaiTro { get; set; }
        public DiaChiRequest DiaChi { get; set; }
    }
}
