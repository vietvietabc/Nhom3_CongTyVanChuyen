using Microsoft.AspNetCore.Http;
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
    public class DanhMucController2 : ControllerBase
    {
        private readonly MyDbContext _context;

        public DanhMucController2(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/DanhMucController2
        [HttpGet]
        public async Task<ActionResult> GetAllDanhMuc()
        {
            try
            {
                var danhMucs = await _context.DanhMucs
                    .Select(dm => new
                    {
                        dm.MaDanhMuc,
                        dm.TenDanhMuc,
                        dm.MoTa
                    })
                    .ToListAsync();

                return Ok(danhMucs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách danh mục: " + ex.Message });
            }
        }

        // GET: api/DanhMucController2/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDanhMucById(string id)
        {
            try
            {
                var danhMuc = await _context.DanhMucs
                    .Where(dm => dm.MaDanhMuc == id)
                    .Select(dm => new
                    {
                        dm.MaDanhMuc,
                        dm.TenDanhMuc,
                        dm.MoTa
                    })
                    .FirstOrDefaultAsync();

                if (danhMuc == null)
                    return NotFound(new { message = "Không tìm thấy danh mục" });

                return Ok(danhMuc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin danh mục: " + ex.Message });
            }
        }
    }
}
