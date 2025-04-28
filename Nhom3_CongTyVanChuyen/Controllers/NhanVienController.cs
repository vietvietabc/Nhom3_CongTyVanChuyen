using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using Nhom3_CongTyVanChuyen.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nhom3_CongTyVanChuyen.DTOs.Requests;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class NhanVienController : ControllerBase
    {
        private readonly MyDbContext _context;

        public NhanVienController(MyDbContext context)
        {
            _context = context;
        }

        // GET all
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVienDto>>> GetAll()
        {
            var list = await _context.NhanViens
                .Include(x => x.VaiTro)
                .Include(x => x.SoNha)
                .Select(nv => new NhanVienDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    TenNhanVien = nv.TenNhanVien,
                    Email = nv.Email,
                    MatKhau = nv.MatKhau,
                    SDT = nv.SDT,
                    CCCD = nv.CCCD,
                    MaVaiTro = nv.MaVaiTro,
                    MaSoNha = nv.MaSoNha,
                    TenVaiTro = nv.VaiTro.TenVaiTro,
                    DiaChiSoNha = nv.SoNha.DiaChiSoNha
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<NhanVienDto>> GetById(string id)
        {
            var nv = await _context.NhanViens
                .Include(x => x.VaiTro)
                .Include(x => x.SoNha)
                .SingleOrDefaultAsync(x => x.MaNhanVien == id);

            if (nv == null) return NotFound();

            var dto = new NhanVienDto
            {
                MaNhanVien = nv.MaNhanVien,
                TenNhanVien = nv.TenNhanVien,
                Email = nv.Email,
                MatKhau = nv.MatKhau,
                SDT = nv.SDT,
                CCCD = nv.CCCD,
                MaVaiTro = nv.MaVaiTro,
                MaSoNha = nv.MaSoNha,
                TenVaiTro = nv.VaiTro?.TenVaiTro,
                DiaChiSoNha = nv.SoNha?.DiaChiSoNha
            };

            return Ok(dto);
        }

        // Updated Create method to split complex await expressions into separate statements
        [HttpPost]
        public async Task<ActionResult<NhanVienDto>> Create(NhanVienRequest request)
        {
            try
            {
                // Step 1: Generate MaSoNha
                var maSoNha = "SN" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

                var diaChiDayDu = await GetDiaChiDayDu(request);
                var soNha = new SoNha
                {
                    MaSoNha = maSoNha,
                    DiaChiSoNha = $"{request.DiaChi.SoNha}, {diaChiDayDu}",
                    MaPhuongXa = request.DiaChi.MaPhuongXa.ToString() 
                };

                _context.SoNhas.Add(soNha);
                await _context.SaveChangesAsync();

                // Step 2: Generate MaNhanVien
                var count = await _context.NhanViens.CountAsync(x => x.MaVaiTro == request.MaVaiTro);
                var newCode = $"{request.MaVaiTro}{(count + 1).ToString("D3")}";

                var nv = new NhanVien
                {
                    MaNhanVien = newCode,
                    TenNhanVien = request.TenNhanVien,
                    Email = request.Email,
                    MatKhau = request.MatKhau,
                    SDT = request.SDT,
                    CCCD = request.CCCD,
                    MaVaiTro = request.MaVaiTro,
                    MaSoNha = soNha.MaSoNha
                };

                _context.NhanViens.Add(nv);
                await _context.SaveChangesAsync();

                // Step 3: Load related entities
                await _context.Entry(nv).Reference(x => x.VaiTro).LoadAsync();
                await _context.Entry(nv).Reference(x => x.SoNha).LoadAsync();

                var dto = new NhanVienDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    TenNhanVien = nv.TenNhanVien,
                    Email = nv.Email,
                    MatKhau = nv.MatKhau,
                    SDT = nv.SDT,
                    CCCD = nv.CCCD,
                    MaVaiTro = nv.MaVaiTro,
                    MaSoNha = nv.MaSoNha,
                    TenVaiTro = nv.VaiTro?.TenVaiTro,
                    DiaChiSoNha = nv.SoNha?.DiaChiSoNha
                };

                return CreatedAtAction(nameof(GetById), new { id = nv.MaNhanVien }, dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }


        // PUT -> cập nhật nhân viên
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, NhanVienDto updateDto)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return NotFound();

            nv.MaVaiTro = updateDto.MaVaiTro;
            nv.TenNhanVien = updateDto.TenNhanVien;
            nv.MatKhau = updateDto.MatKhau;
            nv.Email = updateDto.Email;
            nv.SDT = updateDto.SDT;
            nv.MaSoNha = updateDto.MaSoNha;
            nv.CCCD = updateDto.CCCD;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE -> xóa nhân viên
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return NotFound();

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Support function: ghép địa chỉ đầy đủ

        private async Task<string> GetDiaChiDayDu(NhanVienRequest request)
        {
            var xa = await _context.PhuongXas.FindAsync(request.DiaChi.MaPhuongXa);
            var quan = await _context.QuanHuyens.FindAsync(request.DiaChi.MaQuanHuyen);
            var tinh = await _context.TinhThanhPhos.FindAsync(request.DiaChi.MaTinhTP);

            return $"{xa?.TenPhuongXa ?? "Chưa rõ"}, {quan?.TenQuanHuyen ?? "Chưa rõ"}, {tinh?.TenTinhTP ?? "Chưa rõ"}";
        }


    }
}
