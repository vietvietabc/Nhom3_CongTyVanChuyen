public class HangHoaDonHangDto
{
    public string? MaDanhMuc { get; set; }
    public string TenDanhMuc { get; set; }// Danh mục đã có trong DB (chọn từ combobox)
    public string TinhChatHangHoa { get; set; }     // Tên tính chất (đã có sẵn)
    public double DonGia { get; set; }
    public int SoLuong { get; set; }
    public double TrongLuong { get; set; }          // Đơn vị: kg
    public string KichThuoc { get; set; }           // Ví dụ: "10x20x30cm"
}
