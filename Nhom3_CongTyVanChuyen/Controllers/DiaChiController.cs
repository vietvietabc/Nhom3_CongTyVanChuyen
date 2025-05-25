using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiaChiController : ControllerBase
    {
        private readonly MyDbContext _context;

        public DiaChiController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/DiaChi/TinhThanhPho
        [HttpGet("TinhThanhPho")]
        public async Task<IActionResult> GetTinhThanhPho()
        {
            var tinhTPs = await _context.TinhThanhPhos
                .OrderBy(t => t.TenTinhTP)
                .ToListAsync();

            return Ok(tinhTPs);
        }

        // GET: api/DiaChi/QuanHuyen?maTinhTP=...
        [HttpGet("QuanHuyen")]
        public async Task<IActionResult> GetQuanHuyen(string maTinhTP)
        {
            if (string.IsNullOrEmpty(maTinhTP))
            {
                return BadRequest("Mã tỉnh/thành phố không được để trống");
            }

            var quanHuyens = await _context.QuanHuyens
                .Where(q => q.MaTinhTP == maTinhTP)
                .OrderBy(q => q.TenQuanHuyen)
                .ToListAsync();

            return Ok(quanHuyens);
        }

        // GET: api/DiaChi/PhuongXa?maQuanHuyen=...
        [HttpGet("PhuongXa")]
        public async Task<IActionResult> GetPhuongXa(string maQuanHuyen)
        {
            if (string.IsNullOrEmpty(maQuanHuyen))
            {
                return BadRequest("Mã quận/huyện không được để trống");
            }

            var phuongXas = await _context.PhuongXas
                .Where(p => p.MaQuanHuyen == maQuanHuyen)
                .OrderBy(p => p.TenPhuongXa)
                .ToListAsync();

            return Ok(phuongXas);
        }

        // GET: api/DiaChi/SoNhaInfo?maSoNha=...
        [HttpGet("SoNhaInfo")]
        public async Task<IActionResult> GetFullAddressBySoNha(string maSoNha)
        {
            if (string.IsNullOrEmpty(maSoNha))
                return BadRequest("Mã số nhà không được để trống");

            var soNha = await _context.SoNhas
                .Include(s => s.PhuongXa)
                    .ThenInclude(p => p.QuanHuyen)
                        .ThenInclude(q => q.TinhThanhPho)
                .FirstOrDefaultAsync(s => s.MaSoNha == maSoNha);

            if (soNha == null)
                return NotFound(new { message = "Không tìm thấy thông tin địa chỉ với mã số nhà đã cung cấp." });

            var result = new
            {
                maSoNha = soNha.MaSoNha,
                diaChi = soNha.DiaChiSoNha,
                tenPhuongXa = soNha.PhuongXa?.TenPhuongXa,
                maPhuongXa = soNha.PhuongXa?.MaPhuongXa,
                tenQuanHuyen = soNha.PhuongXa?.QuanHuyen?.TenQuanHuyen,
                maQuanHuyen = soNha.PhuongXa?.QuanHuyen?.MaQuanHuyen,
                tenTinhTP = soNha.PhuongXa?.QuanHuyen?.TinhThanhPho?.TenTinhTP,
                maTinhTP = soNha.PhuongXa?.QuanHuyen?.TinhThanhPho?.MaTinhTP
            };

            return Ok(result);
        }
        [HttpGet("GenerateMaSoNha")]
        public IActionResult GenerateMaSoNha()
        {
            string maSoNha = "SN" + DateTime.Now.Ticks.ToString();
            return Ok(maSoNha);
        }
    }
}