using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Dtos
{
    public class DanhMucRequestDto
    {
        // Bỏ Required nếu có
        public string MaDanhMuc { get; set; } = string.Empty; // Mặc định là chuỗi rỗng

        [Required]
        public string TenDanhMuc { get; set; }

        public string MoTa { get; set; }
    }
}