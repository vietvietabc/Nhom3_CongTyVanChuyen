using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using Nhom3_CongTyVanChuyen.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HangHoaController : ControllerBase
    {
        private readonly MyDbContext _context;

        public HangHoaController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/HangHoa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HangHoaDto>>> GetHangHoas([FromQuery] string? search = null, [FromQuery] string? danhMuc = null)
        {
            try
            {
                var query = _context.HangHoas
                    .Include(h => h.DanhMuc)
                    .AsQueryable();

                // Áp dụng filter nếu có
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(h => h.TinhChatHangHoa.Contains(search) || h.MaHangHoa.Contains(search));
                }

                if (!string.IsNullOrEmpty(danhMuc))
                {
                    query = query.Where(h => h.MaDanhMuc == danhMuc);
                }

                var hangHoas = await query.Select(h => new HangHoaDto
                {
                    MaHangHoa = h.MaHangHoa,
                    TinhChatHangHoa = h.TinhChatHangHoa,
                    MaDanhMuc = h.MaDanhMuc,
                    TenDanhMuc = h.DanhMuc.TenDanhMuc,
                    DonGia = h.DonGia
                }).ToListAsync();

                return hangHoas;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error retrieving hang hoa: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/HangHoa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HangHoaDto>> GetHangHoa(string id)
        {
            var hangHoa = await _context.HangHoas
                .Include(h => h.DanhMuc)
                .Where(h => h.MaHangHoa == id)
                .Select(h => new HangHoaDto
                {
                    MaHangHoa = h.MaHangHoa,
                    TinhChatHangHoa = h.TinhChatHangHoa,
                    MaDanhMuc = h.MaDanhMuc,
                    TenDanhMuc = h.DanhMuc.TenDanhMuc,
                    DonGia = h.DonGia
                })
                .FirstOrDefaultAsync();

            if (hangHoa == null)
            {
                return NotFound();
            }

            return hangHoa;
        }

        // GET: api/HangHoa/Count
        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetHangHoaCount()
        {
            return await _context.HangHoas.CountAsync();
        }

        // GET: api/HangHoa/Popular
        [HttpGet("Popular")]
        public async Task<ActionResult<IEnumerable<HangHoaDto>>> GetPopularHangHoa()
        {
            // Lấy top 5 hàng hóa (có thể dựa trên số lượng đơn hàng trong thực tế)
            var hangHoas = await _context.HangHoas
                .Include(h => h.DanhMuc)
                .Take(5)
                .Select(h => new HangHoaDto
                {
                    MaHangHoa = h.MaHangHoa,
                    TinhChatHangHoa = h.TinhChatHangHoa,
                    MaDanhMuc = h.MaDanhMuc,
                    TenDanhMuc = h.DanhMuc.TenDanhMuc,
                    DonGia = h.DonGia
                })
                .ToListAsync();

            return hangHoas;
        }

        // PUT: api/HangHoa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHangHoa(string id, HangHoaUpdateDto hangHoaDto)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            // Kiểm tra danh mục tồn tại
            var danhMuc = await _context.DanhMucs.FindAsync(hangHoaDto.MaDanhMuc);
            if (danhMuc == null)
            {
                return BadRequest("Danh mục không tồn tại!");
            }

            hangHoa.TinhChatHangHoa = hangHoaDto.TinhChatHangHoa;
            hangHoa.MaDanhMuc = hangHoaDto.MaDanhMuc;
            hangHoa.DonGia = hangHoaDto.DonGia;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HangHoaExists(id))
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

        // POST: api/HangHoa
        [HttpPost]
        public async Task<ActionResult<HangHoaDto>> PostHangHoa(HangHoaCreateDto hangHoaDto)
        {
            // Kiểm tra danh mục tồn tại
            var danhMuc = await _context.DanhMucs.FindAsync(hangHoaDto.MaDanhMuc);
            if (danhMuc == null)
            {
                return BadRequest("Danh mục không tồn tại!");
            }

            // Tạo mã hàng hóa tự động: HH001, HH002, ...
            int count = await _context.HangHoas.CountAsync();
            string maHangHoa = $"HH{(count + 1).ToString().PadLeft(3, '0')}";

            var hangHoa = new HangHoa
            {
                MaHangHoa = maHangHoa,
                TinhChatHangHoa = hangHoaDto.TinhChatHangHoa,
                MaDanhMuc = hangHoaDto.MaDanhMuc,
                DonGia = hangHoaDto.DonGia
            };

            _context.HangHoas.Add(hangHoa);
            await _context.SaveChangesAsync();

            var result = new HangHoaDto
            {
                MaHangHoa = hangHoa.MaHangHoa,
                TinhChatHangHoa = hangHoa.TinhChatHangHoa,
                MaDanhMuc = hangHoa.MaDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                DonGia = hangHoa.DonGia
            };

            return CreatedAtAction("GetHangHoa", new { id = hangHoa.MaHangHoa }, result);
        }

        // DELETE: api/HangHoa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHangHoa(string id)
        {
            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            // TODO: Kiểm tra xem hàng hóa có đang được sử dụng trong đơn hàng không
            // Nếu có thì không cho phép xóa

            _context.HangHoas.Remove(hangHoa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HangHoaExists(string id)
        {
            return _context.HangHoas.Any(e => e.MaHangHoa == id);
        }
    }
}