using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nhom3_CongTyVanChuyen.Data
{
    public class TinhThanhPho
    {
        [Key]
        public string MaTinhTP { get; set; }
        public string TenTinhTP { get; set; }

        public ICollection<QuanHuyen> QuanHuyens { get; set; }
    }
}
