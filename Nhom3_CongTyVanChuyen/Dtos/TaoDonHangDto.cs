public class TaoDonHangDto
{
    public string TenDonHang { get; set; }
    public double TienThuHo { get; set; }
    public DateTime NgayGui { get; set; }

    // Khách hàng
    public string TenKhachHang { get; set; }
    public string SDT_KhachHang { get; set; }
    public DiaChiDto DiaChiKhachHang { get; set; }

    // Người nhận
    public string HoTenNguoiNhan { get; set; }
    public string SDT_NguoiNhan { get; set; }
    public DiaChiDto DiaChiNguoiNhan { get; set; }

    // Danh sách hàng hóa
    public List<HangHoaDonHangDto> HangHoas { get; set; }
}
