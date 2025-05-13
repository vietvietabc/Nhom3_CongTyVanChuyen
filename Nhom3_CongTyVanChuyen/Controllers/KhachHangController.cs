using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Dtos;
using Nhom3_CongTyVanChuyen.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KhachHangController : ControllerBase
    {
        private readonly MyDbContext _context;

        public KhachHangController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/KhachHang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetKhachHangs(string search = null)
        {
            var query = _context.KhachHangs
                .Include(k => k.SoNha)
                .ThenInclude(s => s.PhuongXa)
                .ThenInclude(p => p.QuanHuyen)
                .ThenInclude(q => q.TinhThanhPho)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(k =>
                    k.TenKhachHang.ToLower().Contains(search) ||
                    k.SDT.Contains(search) ||
                    k.Email.ToLower().Contains(search) ||
                    k.CCCD.Contains(search)
                );
            }

            var khachHangs = await query.Select(k => new
            {
                k.MaKhachHang,
                k.TenKhachHang,
                k.SDT,
                k.Email,
                k.NgaySinh,
                k.CCCD,
                k.MaSoNha,
                MaTinhTP = k.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP,
                TenTinhTP = k.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP,
                MaQuanHuyen = k.SoNha.PhuongXa.QuanHuyen.MaQuanHuyen,
                TenQuanHuyen = k.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                MaPhuongXa = k.SoNha.PhuongXa.MaPhuongXa,
                TenPhuongXa = k.SoNha.PhuongXa.TenPhuongXa,
                DiaChiSoNha = k.SoNha.DiaChiSoNha,
                DiaChiChiTiet = $"{k.SoNha.DiaChiSoNha}, {k.SoNha.PhuongXa.TenPhuongXa}, {k.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen}, {k.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP}"
            }).ToListAsync();

            return khachHangs;
        }

        // GET: api/KhachHang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetKhachHang(string id)
        {
            var khachHang = await _context.KhachHangs
                .Include(k => k.SoNha)
                .ThenInclude(s => s.PhuongXa)
                .ThenInclude(p => p.QuanHuyen)
                .ThenInclude(q => q.TinhThanhPho)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound();
            }

            var result = new
            {
                khachHang.MaKhachHang,
                khachHang.TenKhachHang,
                khachHang.SDT,
                khachHang.Email,
                khachHang.NgaySinh,
                khachHang.CCCD,
                khachHang.MaSoNha,
                MaTinhTP = khachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP,
                TenTinhTP = khachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP,
                MaQuanHuyen = khachHang.SoNha.PhuongXa.QuanHuyen.MaQuanHuyen,
                TenQuanHuyen = khachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                MaPhuongXa = khachHang.SoNha.PhuongXa.MaPhuongXa,
                TenPhuongXa = khachHang.SoNha.PhuongXa.TenPhuongXa,
                DiaChiSoNha = khachHang.SoNha.DiaChiSoNha,
                DiaChiChiTiet = $"{khachHang.SoNha.DiaChiSoNha}, {khachHang.SoNha.PhuongXa.TenPhuongXa}, {khachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen}, {khachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP}"
            };

            return result;
        }

        // POST: api/KhachHang
        [HttpPost]
        public async Task<ActionResult<KhachHang>> CreateKhachHang([FromBody] KhachHangCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tạo SoNha mới
            var soNha = new SoNha
            {
                MaSoNha = "SN" + Guid.NewGuid().ToString().Substring(0, 8),
                MaPhuongXa = dto.MaPhuongXa,
                DiaChiSoNha = dto.DiaChiSoNha
            };

            _context.SoNhas.Add(soNha);

            // Tạo KhachHang mới
            var khachHang = new KhachHang
            {
                MaKhachHang = "KH" + Guid.NewGuid().ToString().Substring(0, 8),
                TenKhachHang = dto.TenKhachHang,
                SDT = dto.SDT,
                Email = dto.Email,
                MatKhau = dto.MatKhau, // Trong thực tế, cần mã hóa mật khẩu
                NgaySinh = dto.NgaySinh,
                CCCD = dto.CCCD,
                MaSoNha = soNha.MaSoNha
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKhachHang), new { id = khachHang.MaKhachHang }, khachHang);
        }

        // PUT: api/KhachHang/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKhachHang(string id, [FromBody] KhachHangUpdateDto dto)
        {
            if (id != dto.MaKhachHang)
            {
                return BadRequest("ID không khớp");
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cá nhân
            khachHang.TenKhachHang = dto.TenKhachHang;
            khachHang.SDT = dto.SDT;
            khachHang.Email = dto.Email;
            khachHang.NgaySinh = dto.NgaySinh;
            khachHang.CCCD = dto.CCCD;

            // Tìm SoNha hiện tại
            var soNha = await _context.SoNhas.FindAsync(khachHang.MaSoNha);

            // Cập nhật thông tin địa chỉ
            soNha.MaPhuongXa = dto.MaPhuongXa;
            soNha.DiaChiSoNha = dto.DiaChiSoNha;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhachHangExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/KhachHang/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhachHang(string id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            // Kiểm tra xem khách hàng đã có đơn hàng chưa
            var hasOrders = await _context.DonHangs.AnyAsync(d => d.MaKhachHang == id);
            if (hasOrders)
            {
                return BadRequest("Không thể xóa khách hàng đã có đơn hàng");
            }

            // Lấy mã số nhà để xóa sau
            var maSoNha = khachHang.MaSoNha;

            // Xóa khách hàng
            _context.KhachHangs.Remove(khachHang);

            // Xóa địa chỉ kèm theo
            var soNha = await _context.SoNhas.FindAsync(maSoNha);
            if (soNha != null)
            {
                _context.SoNhas.Remove(soNha);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/KhachHang/Count
        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetKhachHangCount()
        {
            return await _context.KhachHangs.CountAsync();
        }

        private bool KhachHangExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaKhachHang == id);
        }
    }


}