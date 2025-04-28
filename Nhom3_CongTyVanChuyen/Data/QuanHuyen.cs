using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class QuanHuyen
    {
        [Key]
        public string MaQuanHuyen { get; set; }

        public string MaTinhTP { get; set; }
        public string TenQuanHuyen { get; set; }

        [ForeignKey("MaTinhTP")]
        public TinhThanhPho TinhThanhPho { get; set; }

        public ICollection<PhuongXa> PhuongXas { get; set; }
    }
}
