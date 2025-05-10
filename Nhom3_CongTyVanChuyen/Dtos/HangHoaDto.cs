using System;

namespace Nhom3_CongTyVanChuyen.Dtos
{
    public class HangHoaDto
    {
        public string MaHangHoa { get; set; }
        public string TinhChatHangHoa { get; set; }
        public string MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; } 
        public double DonGia { get; set; }
    }

    public class HangHoaCreateDto
    {
        public string TinhChatHangHoa { get; set; }
        public string MaDanhMuc { get; set; }
        public double DonGia { get; set; }
    }

    public class HangHoaUpdateDto
    {
        public string TinhChatHangHoa { get; set; }
        public string MaDanhMuc { get; set; }
        public double DonGia { get; set; }
    }
}