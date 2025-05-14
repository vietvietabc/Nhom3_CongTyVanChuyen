using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using Nhom3_CongTyVanChuyen.Services;
using Nhom3_CongTyVanChuyen.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly MyDbContext _context;

        public NhanVienController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/NhanVien
        [HttpGet]
        public async Task<IActionResult> GetNhanViens(string search = "", string vaiTro = "")
        {
            try
            {
                var query = _context.NhanViens
                    .Include(nv => nv.VaiTro)
                    .Include(nv => nv.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                    .ThenInclude(px => px.QuanHuyen)
                    .ThenInclude(qh => qh.TinhThanhPho)
                    .AsQueryable();

                // Áp dụng filter
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(nv =>
                        nv.TenNhanVien.ToLower().Contains(search) ||
                        nv.Email.ToLower().Contains(search) ||
                        nv.SDT.Contains(search) ||
                        nv.MaNhanVien.ToLower().Contains(search)
                    );
                }

                if (!string.IsNullOrEmpty(vaiTro))
                {
                    query = query.Where(nv => nv.MaVaiTro == vaiTro);
                }

                // Convert to DTOs to avoid circular references
                var nhanViens = await query
                    .Select(nv => new
                    {
                        nv.MaNhanVien,
                        nv.TenNhanVien,
                        nv.MaVaiTro,
                        TenVaiTro = nv.VaiTro.TenVaiTro,
                        nv.Email,
                        SDT = nv.SDT,
                        DiaChiSoNha = nv.SoNha.DiaChiSoNha,
                        nv.CCCD
                    })
                    .ToListAsync();

                return Ok(nhanViens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách nhân viên: {ex.Message}");
            }
        }

        // GET: api/NhanVien/Count
        [HttpGet("Count")]
        public async Task<IActionResult> GetNhanVienCount()
        {
            try
            {
                int count = await _context.NhanViens.CountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi đếm số nhân viên: {ex.Message}");
            }
        }

        // GET: api/NhanVien/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNhanVien(string id)
        {
            try
            {
                var nhanVien = await _context.NhanViens
                    .Include(nv => nv.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                    .ThenInclude(px => px.QuanHuyen)
                    .ThenInclude(qh => qh.TinhThanhPho)
                    .Where(nv => nv.MaNhanVien == id)
                    .Select(nv => new
                    {
                        nv.MaNhanVien,
                        nv.TenNhanVien,
                        nv.MaVaiTro,
                        nv.Email,
                        SDT = nv.SDT,
                        MaTinhTP = nv.SoNha.PhuongXa.QuanHuyen.MaTinhTP,
                        TenTinhTP = nv.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP,
                        MaQuanHuyen = nv.SoNha.PhuongXa.MaQuanHuyen,
                        TenQuanHuyen = nv.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                        MaPhuongXa = nv.SoNha.MaPhuongXa,
                        TenPhuongXa = nv.SoNha.PhuongXa.TenPhuongXa,
                        DiaChiSoNha = nv.SoNha.DiaChiSoNha,
                        nv.CCCD
                    })
                    .FirstOrDefaultAsync();

                if (nhanVien == null)
                {
                    return NotFound($"Không tìm thấy nhân viên với mã {id}");
                }

                return Ok(nhanVien);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tìm nhân viên: {ex.Message}");
            }
        }

        // POST: api/NhanVien
        [HttpPost]
        public async Task<IActionResult> CreateNhanVien([FromBody] CreateNhanVienDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Kiểm tra phường/xã có tồn tại không
                var phuongXa = await _context.PhuongXas.FindAsync(dto.MaPhuongXa);
                if (phuongXa == null)
                {
                    return BadRequest("Phường/xã không tồn tại");
                }

                // 1. Xử lý số nhà - tìm hoặc tạo mới
                SoNha soNha = await _context.SoNhas
                    .FirstOrDefaultAsync(s => s.MaPhuongXa == dto.MaPhuongXa && s.DiaChiSoNha == dto.DiaChiSoNha);

                if (soNha == null)
                {
                    // Tạo số nhà mới nếu chưa có
                    soNha = new SoNha
                    {
                        MaSoNha = "SN" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                        MaPhuongXa = dto.MaPhuongXa,
                        DiaChiSoNha = dto.DiaChiSoNha
                    };

                    _context.SoNhas.Add(soNha);
                    await _context.SaveChangesAsync();
                }

                // 2. Tạo mã nhân viên theo format: [MaVaiTro]_[Random]
                string randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                string maNhanVien = $"{dto.MaVaiTro}_{randomPart}";

                // 3. Tạo nhân viên mới
                var nhanVien = new NhanVien
                {
                    MaNhanVien = maNhanVien,
                    TenNhanVien = dto.TenNhanVien,
                    MaVaiTro = dto.MaVaiTro,
                    MatKhau = dto.MatKhau, // Trong thực tế nên mã hóa mật khẩu
                    Email = dto.Email,
                    SDT = dto.SDT,
                    MaSoNha = soNha.MaSoNha,
                    CCCD = dto.CCCD,
                    NguoiTao = LoginInfo.CurrentUserId,     // vietvietabc
                    NgayTao = LoginInfo.CurrentTime,        // Current time
                    NguoiCapNhat = LoginInfo.CurrentUserId, // vietvietabc
                    NgayCapNhat = LoginInfo.CurrentTime     // Current time
                };

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                // Return a simplified DTO instead of the entity to avoid circular references
                var result = new
                {
                    nhanVien.MaNhanVien,
                    nhanVien.TenNhanVien,
                    nhanVien.MaVaiTro,
                    nhanVien.Email,
                    nhanVien.SDT,
                    MaSoNha = soNha.MaSoNha,
                    DiaChiSoNha = soNha.DiaChiSoNha,
                    nhanVien.CCCD
                };

                return CreatedAtAction(nameof(GetNhanVien), new { id = nhanVien.MaNhanVien }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Thêm nhân viên thất bại: {ex.Message}");
            }
        }

        // PUT: api/NhanVien/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNhanVien(string id, [FromBody] UpdateNhanVienDto dto)
        {
            if (id != dto.MaNhanVien)
            {
                return BadRequest("Mã nhân viên không khớp");
            }

            try
            {
                var nhanVien = await _context.NhanViens.FindAsync(id);
                if (nhanVien == null)
                {
                    return NotFound($"Không tìm thấy nhân viên với mã {id}");
                }

                // 1. Xử lý số nhà - tìm hoặc tạo mới nếu khác với số nhà hiện tại
                SoNha soNha = await _context.SoNhas
                    .FirstOrDefaultAsync(s => s.MaPhuongXa == dto.MaPhuongXa && s.DiaChiSoNha == dto.DiaChiSoNha);

                if (soNha == null)
                {
                    // Tạo số nhà mới
                    soNha = new SoNha
                    {
                        MaSoNha = "SN" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                        MaPhuongXa = dto.MaPhuongXa,
                        DiaChiSoNha = dto.DiaChiSoNha
                    };

                    _context.SoNhas.Add(soNha);
                    await _context.SaveChangesAsync();
                }

                // 2. Cập nhật thông tin nhân viên
                nhanVien.TenNhanVien = dto.TenNhanVien;
                nhanVien.MaVaiTro = dto.MaVaiTro;
                nhanVien.Email = dto.Email;
                nhanVien.SDT = dto.SDT;
                nhanVien.MaSoNha = soNha.MaSoNha;
                nhanVien.CCCD = dto.CCCD;
                nhanVien.NguoiCapNhat = LoginInfo.CurrentUserId;
                nhanVien.NgayCapNhat = LoginInfo.CurrentTime;

                await _context.SaveChangesAsync();

                // Return a simplified DTO instead of the entity
                var result = new
                {
                    nhanVien.MaNhanVien,
                    nhanVien.TenNhanVien,
                    nhanVien.MaVaiTro,
                    nhanVien.Email,
                    nhanVien.SDT,
                    MaSoNha = soNha.MaSoNha,
                    DiaChiSoNha = soNha.DiaChiSoNha,
                    nhanVien.CCCD
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Cập nhật nhân viên thất bại: {ex.Message}");
            }
        }

        // DELETE: api/NhanVien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhanVien(string id)
        {
            try
            {
                var nhanVien = await _context.NhanViens.FindAsync(id);
                if (nhanVien == null)
                {
                    return NotFound($"Không tìm thấy nhân viên với mã {id}");
                }

                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Đã xóa nhân viên {id} thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Xóa nhân viên thất bại: {ex.Message}");
            }
        }

        private bool NhanVienExists(string id)
        {
            return _context.NhanViens.Any(e => e.MaNhanVien == id);
        }
    }

    
}