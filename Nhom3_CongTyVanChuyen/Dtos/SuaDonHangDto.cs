namespace Nhom3_CongTyVanChuyen.Dtos
{
    public class SuaDonHangDto
    {
        public string TenDonHang { get; set; }
        public double TienThuHo { get; set; }
        public double PhiGiaoHang { get; set; }
        public DateTime NgayGui { get; set; }

        public string TenKhachHang { get; set; }
        public string SDT_KhachHang { get; set; }
        public DiaChiDto DiaChiKhachHang { get; set; }

        public string HoTenNguoiNhan { get; set; }
        public string SDT_NguoiNhan { get; set; }
        public DiaChiDto DiaChiNguoiNhan { get; set; }
        public string? NguoiTraPhi { get; set; }
        public List<HangHoaDto> HangHoas { get; set; }
    }
}
