using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class PhuongXa
    {
        [Key]
        public string MaPhuongXa { get; set; }
        public string MaQuanHuyen { get; set; }
        public string TenPhuongXa { get; set; }

        [ForeignKey("MaQuanHuyen")]
        public QuanHuyen QuanHuyen { get; set; }

        public ICollection<SoNha> SoNhas { get; set; }
    }
}
