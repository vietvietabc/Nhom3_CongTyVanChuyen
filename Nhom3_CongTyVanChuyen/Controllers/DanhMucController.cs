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
    public class DanhMucController : ControllerBase
    {
        private readonly MyDbContext _context;

        public DanhMucController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/DanhMuc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DanhMucDto>>> GetDanhMucs()
        {
            return await _context.DanhMucs
                .Select(d => new DanhMucDto
                {
                    MaDanhMuc = d.MaDanhMuc,
                    TenDanhMuc = d.TenDanhMuc,
                    MoTa = d.MoTa
                })
                .ToListAsync();
        }

        // GET: api/DanhMuc/DM001
        [HttpGet("{id}")]
        public async Task<ActionResult<DanhMucDto>> GetDanhMuc(string id)
        {
            var danhMuc = await _context.DanhMucs
                .Where(d => d.MaDanhMuc == id)
                .Select(d => new DanhMucDto
                {
                    MaDanhMuc = d.MaDanhMuc,
                    TenDanhMuc = d.TenDanhMuc,
                    MoTa = d.MoTa
                })
                .FirstOrDefaultAsync();

            if (danhMuc == null)
            {
                return NotFound();
            }

            return danhMuc;
        }

        // PUT: api/DanhMuc/DM001
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDanhMuc(string id, DanhMucRequestDto danhMucDto)
        {
            if (id != danhMucDto.MaDanhMuc)
            {
                return BadRequest();
            }

            var danhMuc = await _context.DanhMucs.FindAsync(id);

            if (danhMuc == null)
            {
                return NotFound();
            }

            // Chỉ cập nhật các trường cần thiết
            danhMuc.TenDanhMuc = danhMucDto.TenDanhMuc;
            danhMuc.MoTa = danhMucDto.MoTa;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DanhMucExists(id))
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

        // POST: api/DanhMuc
        [HttpPost]
        public async Task<ActionResult<DanhMucDto>> PostDanhMuc(DanhMucRequestDto danhMucDto)
        {
            var danhMuc = new DanhMuc
            {
                MaDanhMuc = string.IsNullOrEmpty(danhMucDto.MaDanhMuc) ? null : danhMucDto.MaDanhMuc, // Cho phép null hoặc empty
                TenDanhMuc = danhMucDto.TenDanhMuc,
                MoTa = danhMucDto.MoTa
            };

            // Tạo mã danh mục tự động nếu chưa có
            if (string.IsNullOrEmpty(danhMuc.MaDanhMuc))
            {
                // Đếm số lượng danh mục hiện tại để tạo mã mới
                int count = await _context.DanhMucs.CountAsync();
                string newId = $"DM{(count + 1).ToString().PadLeft(3, '0')}";
                danhMuc.MaDanhMuc = newId;
            }

            _context.DanhMucs.Add(danhMuc);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DanhMucExists(danhMuc.MaDanhMuc))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            var resultDto = new DanhMucDto
            {
                MaDanhMuc = danhMuc.MaDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                MoTa = danhMuc.MoTa
            };

            return CreatedAtAction("GetDanhMuc", new { id = danhMuc.MaDanhMuc }, resultDto);
        }

        // DELETE: api/DanhMuc/DM001
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhMuc(string id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            // Kiểm tra xem danh mục có sản phẩm không
            bool hasProducts = await _context.HangHoas.AnyAsync(h => h.MaDanhMuc == id);
            if (hasProducts)
            {
                return BadRequest("Không thể xóa danh mục đang chứa sản phẩm!");
            }

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DanhMucExists(string id)
        {
            return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
        }
    }
}