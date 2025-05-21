using Microsoft.AspNetCore.Mvc;
using Nhom3_CongTyVanChuyen.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangDangNhapController : ControllerBase
    {
        private readonly MyDbContext _context;

        public KhachHangDangNhapController(MyDbContext context)
        {
            _context = context;
        }

        // Phương thức đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string Email, [FromForm] string MatKhau)
        {
            // Kiểm tra thông tin nhập vào
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MatKhau))
            {
                return BadRequest("Vui lòng nhập email và mật khẩu.");
            }

            // Kiểm tra xem email có tồn tại trong hệ thống hay không
            var khachHang = _context.KhachHangs
                .FirstOrDefault(kh => kh.Email == Email);

            if (khachHang == null)
            {
                return Unauthorized("Email không tồn tại.");
            }

            // Kiểm tra mật khẩu có khớp không (Có thể mã hóa mật khẩu trong thực tế)
            if (khachHang.MatKhau != MatKhau)
            {
                return Unauthorized("Mật khẩu không đúng.");
            }

            // Nếu đăng nhập thành công
            return Ok(new { message = "Đăng nhập thành công!", maKhachHang = khachHang.MaKhachHang });
        }
    }
}
