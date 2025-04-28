using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class DanhMuc
    {
        [Key]
        public string MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }

        public ICollection<HangHoa> HangHoas { get; set; }
    }
}
