// File: Controllers/LoginController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using Nhom3_CongTyVanChuyen.Dtos;
using Nhom3_CongTyVanChuyen.Services;
using System;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly MyDbContext _context;

        public LoginController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var nv = await _context.NhanViens
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x => x.Email == dto.Email && x.MatKhau == dto.MatKhau);

            if (nv == null)
                return Unauthorized(new { message = "Sai email hoặc mật khẩu!" });

            // Thiết lập thông tin người đăng nhập hiện tại
            LoginInfo.CurrentUserId = nv.MaNhanVien; // Mã nhân viên của người đăng nhập
            LoginInfo.CurrentTime = DateTime.Parse("2025-05-09 11:16:06"); // Thời gian hiện tại

            return Ok(new
            {
                MaNhanVien = nv.MaNhanVien,
                TenNhanVien = nv.TenNhanVien,
                Email = nv.Email,
                MaVaiTro = nv.VaiTro.MaVaiTro,
                TenVaiTro = nv.VaiTro.TenVaiTro
            });
        }
    }
}