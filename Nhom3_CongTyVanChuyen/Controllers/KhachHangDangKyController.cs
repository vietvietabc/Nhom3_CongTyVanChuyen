using Microsoft.AspNetCore.Mvc;
using Nhom3_CongTyVanChuyen.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangDangKyController : ControllerBase
    {
        private readonly MyDbContext _context;

        public KhachHangDangKyController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string TenKhachHang, [FromForm] string SDT, [FromForm] string Email, [FromForm] string MatKhau)
        {
            if (string.IsNullOrWhiteSpace(TenKhachHang) || string.IsNullOrWhiteSpace(SDT) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MatKhau))
            {
                return BadRequest("Vui lòng nhập đầy đủ thông tin.");
            }

            var listMaKH = _context.KhachHangs
                .Select(k => k.MaKhachHang)
                .Where(m => m.StartsWith("KH"))
                .ToList();

            int maxMaKH = 0;
            foreach (var m in listMaKH)
            {
                if (int.TryParse(m.Substring(2), out int num))
                {
                    if (num > maxMaKH)
                        maxMaKH = num;
                }
            }

            var maKH = "KH" + (maxMaKH + 1).ToString("D3");


            var kh = new KhachHang
            {
                MaKhachHang = maKH,
                TenKhachHang = TenKhachHang,
                SDT = SDT,
                Email = Email,
                MatKhau = MatKhau,
                CCCD = "", // để tránh lỗi NULL
                MaSoNha = null,
                NgaySinh = null
            };

            _context.KhachHangs.Add(kh);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!", maKhachHang = maKH });
        }
    }
}
