using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Nhom3_CongTyVanChuyen.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController2 : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<KhachHangController2> _logger;

        public KhachHangController2(MyDbContext context, ILogger<KhachHangController2> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/KhachHangController2/GetInfo
        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    // Nếu không có email trong query string, thử lấy từ query string
                    email = Request.Query["email"].ToString();
                    _logger.LogInformation($"Email from query string: {email}");

                    if (string.IsNullOrEmpty(email))
                    {
                        return BadRequest(new { message = "Email không được để trống" });
                    }
                }

                _logger.LogInformation($"Email: {email}");

                // Tìm khách hàng theo email
                var khachHang = await _context.KhachHangs
                    .Include(k => k.SoNha)
                    .Where(k => k.Email == email)
                    .FirstOrDefaultAsync();

                if (khachHang == null)
                {
                    _logger.LogWarning($"Không tìm thấy khách hàng với email: {email}");
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                _logger.LogInformation($"Tìm thấy khách hàng: {khachHang.MaKhachHang}");

                // Trả về thông tin khách hàng
                var result = new
                {
                    maKhachHang = khachHang.MaKhachHang,
                    tenKhachHang = khachHang.TenKhachHang,
                    email = khachHang.Email,
                    sdt = khachHang.SDT,
                    ngaySinh = khachHang.NgaySinh,
                    cccd = khachHang.CCCD,
                    diaChi = khachHang.SoNha?.DiaChiSoNha,
                    tinhThanh = khachHang.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho?.TenTinhTP,
                    quanHuyen = khachHang.SoNha?.PhuongXa?.QuanHuyen?.TenQuanHuyen,
                    phuongXa = khachHang.SoNha?.PhuongXa?.TenPhuongXa,
                    duong = khachHang.SoNha?.DiaChiSoNha
                };

                _logger.LogInformation($"Trả về dữ liệu: {JsonSerializer.Serialize(result)}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi trong GetInfo: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // GET: api/KhachHangController2/GetByEmail?email=example@email.com
        [HttpGet("GetByEmail")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                _logger.LogInformation($"GetByEmail được gọi với email: {email}");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email không được cung cấp");
                    return BadRequest(new { message = "Email không được để trống" });
                }

                // Tìm khách hàng theo email
                var khachHang = await _context.KhachHangs
                    .Include(k => k.SoNha)
                        .ThenInclude(s => s.PhuongXa)
                            .ThenInclude(p => p.QuanHuyen)
                                .ThenInclude(q => q.TinhThanhPho)
                    .Where(k => k.Email == email)
                    .FirstOrDefaultAsync();

                if (khachHang == null)
                {
                    _logger.LogWarning($"Không tìm thấy khách hàng với email: {email}");
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                _logger.LogInformation($"Tìm thấy khách hàng: {khachHang.MaKhachHang}");

                // Trả về thông tin khách hàng
                var result = new
                {
                    maKhachHang = khachHang.MaKhachHang,
                    tenKhachHang = khachHang.TenKhachHang,
                    email = khachHang.Email,
                    sdt = khachHang.SDT,
                    ngaySinh = khachHang.NgaySinh,
                    cccd = khachHang.CCCD,
                    diaChi = khachHang.SoNha?.DiaChiSoNha,
                    tinhThanh = khachHang.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho?.TenTinhTP,
                    quanHuyen = khachHang.SoNha?.PhuongXa?.QuanHuyen?.TenQuanHuyen,
                    phuongXa = khachHang.SoNha?.PhuongXa?.TenPhuongXa,
                    duong = khachHang.SoNha?.DiaChiSoNha
                };

                _logger.LogInformation($"Trả về dữ liệu: {JsonSerializer.Serialize(result)}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi trong GetByEmail: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // PUT: api/KhachHangController2/UpdateInfo
        // PUT: api/KhachHangController2/UpdateInfo
        [HttpPut("UpdateInfo")]
        public async Task<IActionResult> UpdateInfo([FromBody] KhachHangUpdateModel model)
        {
            try
            {
                _logger.LogInformation($"UpdateInfo được gọi với dữ liệu: {JsonSerializer.Serialize(model)}");

                // Lấy email từ model
                var email = model.Email;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email không được cung cấp");
                    return BadRequest(new { message = "Email không được để trống" });
                }

                _logger.LogInformation($"Email từ model: {email}");

                // Tìm khách hàng theo email
                var khachHang = await _context.KhachHangs
                    .Include(k => k.SoNha)
                        .ThenInclude(s => s.PhuongXa)
                    .Where(k => k.Email == email)
                    .FirstOrDefaultAsync();

                if (khachHang == null)
                {
                    _logger.LogWarning($"Không tìm thấy khách hàng với email: {email}");
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                _logger.LogInformation($"Tìm thấy khách hàng: {khachHang.MaKhachHang}");

                // Cập nhật thông tin khách hàng
                khachHang.TenKhachHang = model.TenKhachHang ?? khachHang.TenKhachHang;
                khachHang.SDT = model.SDT ?? khachHang.SDT;

                // Xử lý ngày sinh nếu có
                if (model.NgaySinh.HasValue)
                {
                    khachHang.NgaySinh = model.NgaySinh.Value;
                }

                khachHang.CCCD = model.CMND ?? khachHang.CCCD;

                // Xử lý thông tin địa chỉ
                if (!string.IsNullOrEmpty(model.MaSoNha) && !string.IsNullOrEmpty(model.DiaChi) && !string.IsNullOrEmpty(model.PhuongXa))
                {
                    // Kiểm tra SoNha đã tồn tại chưa
                    var soNha = await _context.SoNhas.FindAsync(model.MaSoNha);

                    if (soNha == null)
                    {
                        // Tạo mới SoNha
                        soNha = new SoNha
                        {
                            MaSoNha = model.MaSoNha,
                            MaPhuongXa = model.PhuongXa,  // Đã được gửi đúng mã từ client
                            DiaChiSoNha = model.DiaChi
                        };
                        _context.SoNhas.Add(soNha);
                        _logger.LogInformation($"Đã tạo mới SoNha: {model.MaSoNha}");
                    }
                    else
                    {
                        // Cập nhật SoNha
                        soNha.MaPhuongXa = model.PhuongXa;
                        soNha.DiaChiSoNha = model.DiaChi;
                        _logger.LogInformation($"Đã cập nhật SoNha: {model.MaSoNha}");
                    }

                    // Liên kết khách hàng với SoNha
                    khachHang.MaSoNha = model.MaSoNha;
                }

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cập nhật thông tin thành công");

                return Ok(new { message = "Cập nhật thông tin thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi trong UpdateInfo: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
        // Model để nhận dữ liệu cập nhật từ client
        public class KhachHangUpdateModel
        {
            public string Email { get; set; }
            public string TenKhachHang { get; set; }
            public string SDT { get; set; }
            public DateTime? NgaySinh { get; set; }
            public string CMND { get; set; }
            public string DiaChi { get; set; }
            public string TinhThanh { get; set; }
            public string QuanHuyen { get; set; }
            public string PhuongXa { get; set; } // Đây nên là mã phường xã
            public string Duong { get; set; }
            public string MaSoNha { get; set; }
        }

    }
}
