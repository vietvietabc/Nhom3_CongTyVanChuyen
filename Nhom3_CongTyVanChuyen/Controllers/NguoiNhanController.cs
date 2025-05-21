using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiNhanController : ControllerBase
    {
        private readonly MyDbContext _context;

        public NguoiNhanController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/NguoiNhan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetNguoiNhans()
        {
            var result = await _context.NguoiNhans
                .Include(n => n.KhachHang)
                .Include(n => n.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                        .ThenInclude(px => px.QuanHuyen)
                            .ThenInclude(qh => qh.TinhThanhPho)
                .Select(n => new
                {
                    n.MaNguoiNhan,
                    n.HoTen,
                    n.SDT,
                    n.MaKhachHang,
                    TenKhachHang = n.KhachHang != null ? n.KhachHang.TenKhachHang : null,
                    n.MaSoNha,
                    DiaChiDayDu = n.SoNha != null
                        ? string.Join(", ",
                            n.SoNha.DiaChiSoNha,
                            "P." + n.SoNha.PhuongXa.TenPhuongXa,
                            "Q." + n.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                            "TP." + n.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP)
                        : null
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/NguoiNhan/byCustomer/{maKhachHang}
        [HttpGet("byCustomer/{maKhachHang}")]
        public async Task<ActionResult<IEnumerable<object>>> GetNguoiNhansByMaKhachHang(string maKhachHang)
        {
            if (string.IsNullOrEmpty(maKhachHang))
                return BadRequest("Mã khách hàng không hợp lệ");

            var result = await _context.NguoiNhans
                .Where(n => n.MaKhachHang == maKhachHang)
                .Include(n => n.KhachHang)
                .Include(n => n.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                        .ThenInclude(px => px.QuanHuyen)
                            .ThenInclude(qh => qh.TinhThanhPho)
                .Select(n => new
                {
                    n.MaNguoiNhan,
                    n.HoTen,
                    n.SDT,
                    n.MaKhachHang,
                    TenKhachHang = n.KhachHang != null ? n.KhachHang.TenKhachHang : null,
                    n.MaSoNha,
                    DiaChiDayDu = n.SoNha != null
                        ? string.Join(", ",
                            n.SoNha.DiaChiSoNha,
                            "P." + n.SoNha.PhuongXa.TenPhuongXa,
                            "Q." + n.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                            "TP." + n.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP)
                        : null
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/NguoiNhan/NN001
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetNguoiNhanById(string id)
        {
            var n = await _context.NguoiNhans
                .Include(n => n.KhachHang)
                .Include(n => n.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                        .ThenInclude(px => px.QuanHuyen)
                            .ThenInclude(qh => qh.TinhThanhPho)
                .Where(n => n.MaNguoiNhan == id)
                .Select(n => new
                {
                    n.MaNguoiNhan,
                    n.HoTen,
                    n.SDT,
                    n.MaKhachHang,
                    TenKhachHang = n.KhachHang != null ? n.KhachHang.TenKhachHang : null,
                    n.MaSoNha,
                    DiaChiDayDu = n.SoNha != null
                        ? string.Join(", ",
                            n.SoNha.DiaChiSoNha,
                            "P." + n.SoNha.PhuongXa.TenPhuongXa,
                            "Q." + n.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                            "TP." + n.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP)
                        : null
                })
                .FirstOrDefaultAsync();

            if (n == null)
                return NotFound();

            return Ok(n);
        }

        // POST: api/NguoiNhan
        [HttpPost]
        public async Task<ActionResult<NguoiNhan>> PostNguoiNhan(NguoiNhan nguoiNhan)
        {
            if (nguoiNhan == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var exists = await _context.NguoiNhans.AnyAsync(x => x.MaNguoiNhan == nguoiNhan.MaNguoiNhan);
            if (exists)
            {
                return Conflict("Mã người nhận đã tồn tại");
            }

            // Nếu không có mã khách hàng được truyền, trả về lỗi
            if (string.IsNullOrEmpty(nguoiNhan.MaKhachHang))
            {
                return BadRequest("Mã khách hàng không được để trống");
            }

            // Kiểm tra xem mã khách hàng có tồn tại không
            var khachHangExists = await _context.KhachHangs.AnyAsync(kh => kh.MaKhachHang == nguoiNhan.MaKhachHang);
            if (!khachHangExists)
            {
                return BadRequest("Mã khách hàng không tồn tại");
            }

            // Tạo mã người nhận mới nếu chưa có
            if (string.IsNullOrEmpty(nguoiNhan.MaNguoiNhan))
            {
                // Tạo mã người nhận mới dạng NN001, NN002, ...
                var lastNguoiNhan = await _context.NguoiNhans
                    .OrderByDescending(n => n.MaNguoiNhan)
                    .FirstOrDefaultAsync();

                int nextId = 1;
                if (lastNguoiNhan != null && lastNguoiNhan.MaNguoiNhan.StartsWith("NN"))
                {
                    if (int.TryParse(lastNguoiNhan.MaNguoiNhan.Substring(2), out int lastId))
                    {
                        nextId = lastId + 1;
                    }
                }

                nguoiNhan.MaNguoiNhan = $"NN{nextId:D3}";
            }

            _context.NguoiNhans.Add(nguoiNhan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNguoiNhanById), new { id = nguoiNhan.MaNguoiNhan }, nguoiNhan);
        }
    }
}
