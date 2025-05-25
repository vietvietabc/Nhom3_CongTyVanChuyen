using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nhom3_CongTyVanChuyen.Data;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
                //var khachHang = await _context.KhachHangs
                //    .Include(k => k.SoNha)
                //    .Where(k => k.Email == email)
                //    .FirstOrDefaultAsync();
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
                    duong = khachHang.SoNha?.DiaChiSoNha,
                    maSoNha = khachHang.MaSoNha
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
                    duong = khachHang.SoNha?.DiaChiSoNha,
                    maSoNha = khachHang.MaSoNha
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
                if (!string.IsNullOrEmpty(model.NgaySinh))
                {
                    try
                    {
                        khachHang.NgaySinh = DateTime.Parse(model.NgaySinh);
                        _logger.LogInformation($"Đã cập nhật ngày sinh: {model.NgaySinh}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Không thể chuyển đổi ngày sinh: {model.NgaySinh}, lỗi: {ex.Message}");
                    }
                }

                khachHang.CCCD = model.CCCD ?? khachHang.CCCD;

                // Xử lý thông tin địa chỉ
                if (!string.IsNullOrEmpty(model.PhuongXa) && !string.IsNullOrEmpty(model.DiaChi))
                {
                    _logger.LogInformation($"Xử lý thông tin địa chỉ: DiaChi={model.DiaChi}, MaPhuongXa={model.PhuongXa}");

                    // Kiểm tra PhuongXa có tồn tại không
                    var phuongXa = await _context.PhuongXas.FindAsync(model.PhuongXa);
                    if (phuongXa == null)
                    {
                        _logger.LogWarning($"Không tìm thấy phường xã với mã: {model.PhuongXa}");
                        return BadRequest(new { message = $"Không tìm thấy phường xã với mã: {model.PhuongXa}" });
                    }

                    // Tạo hoặc cập nhật SoNha
                    string maSoNha = model.MaSoNha;
                    if (string.IsNullOrEmpty(maSoNha))
                    {
                        // Tạo mã số nhà mới nếu không có
                        maSoNha = "SN" + DateTime.Now.Ticks.ToString();
                        _logger.LogInformation($"Đã tạo mã số nhà mới: {maSoNha}");
                    }

                    // Kiểm tra SoNha đã tồn tại chưa
                    var soNha = await _context.SoNhas.FindAsync(maSoNha);

                    if (soNha == null)
                    {
                        // Tạo mới SoNha
                        soNha = new SoNha
                        {
                            MaSoNha = maSoNha,
                            MaPhuongXa = model.PhuongXa,
                            DiaChiSoNha = model.DiaChi
                        };
                        _context.SoNhas.Add(soNha);
                        _logger.LogInformation($"Đã tạo mới SoNha: {maSoNha}");
                    }
                    else
                    {
                        // Cập nhật SoNha
                        soNha.MaPhuongXa = model.PhuongXa;
                        soNha.DiaChiSoNha = model.DiaChi;
                        _logger.LogInformation($"Đã cập nhật SoNha: {maSoNha}");
                    }

                    // Liên kết khách hàng với SoNha
                    khachHang.MaSoNha = maSoNha;
                    _logger.LogInformation($"Đã liên kết khách hàng {khachHang.MaKhachHang} với SoNha {maSoNha}");
                }
                else
                {
                    _logger.LogWarning("Thiếu thông tin địa chỉ: DiaChi hoặc PhuongXa");
                }

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cập nhật thông tin thành công");

                return Ok(new
                {
                    message = "Cập nhật thông tin thành công",
                    data = new
                    {
                        maKhachHang = khachHang.MaKhachHang,
                        tenKhachHang = khachHang.TenKhachHang,
                        email = khachHang.Email,
                        sdt = khachHang.SDT,
                        ngaySinh = khachHang.NgaySinh,
                        cccd = khachHang.CCCD,
                        maSoNha = khachHang.MaSoNha
                    }
                });
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
            public string NgaySinh { get; set; } // vẫn để string để tiện xử lý parse

            [JsonPropertyName("cccd")] // ánh xạ với trường CCCD trong KhachHang.cs
            public string CCCD { get; set; }

            public string DiaChi { get; set; }
            public string TinhThanh { get; set; }
            public string QuanHuyen { get; set; }
            public string PhuongXa { get; set; }
            public string Duong { get; set; }
            public string MaSoNha { get; set; }
        }

    }
}
