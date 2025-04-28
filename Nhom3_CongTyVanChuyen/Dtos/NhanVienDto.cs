namespace Nhom3_CongTyVanChuyen.Dtos
{
    public class NhanVienDto
    {
        public string? MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public string SDT { get; set; }
        public string CCCD { get; set; }

        // Khóa ngoại
        public string MaVaiTro { get; set; }
        public string MaSoNha { get; set; }

        // Thông tin liên quan
        public string TenVaiTro { get; set; }
        public string DiaChiSoNha { get; set; }
    }
}
