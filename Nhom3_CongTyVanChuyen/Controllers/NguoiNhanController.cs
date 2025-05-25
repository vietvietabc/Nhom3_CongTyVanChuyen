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

        // GET: api/NguoiNhan/GetByKhachHang/{maKhachHang}
        [HttpGet("GetByKhachHang/{maKhachHang}")]
        public async Task<ActionResult<IEnumerable<object>>> GetNguoiNhanByMaKhachHang(string maKhachHang)
        {
            if (string.IsNullOrEmpty(maKhachHang))
                return BadRequest("Mã khách hàng không hợp lệ");

            var result = await _context.NguoiNhans
                .Where(n => n.MaKhachHang == maKhachHang)
                .Include(n => n.SoNha)
                    .ThenInclude(s => s.PhuongXa)
                        .ThenInclude(p => p.QuanHuyen)
                            .ThenInclude(q => q.TinhThanhPho)
                .Select(n => new
                {
                    n.MaNguoiNhan,
                    n.MaKhachHang,
                    n.MaSoNha,
                    n.HoTen,
                    n.SDT,
                    DiaChi = n.SoNha != null ? n.SoNha.DiaChiSoNha : "",
                    PhuongXa = n.SoNha != null && n.SoNha.PhuongXa != null ? n.SoNha.PhuongXa.TenPhuongXa : "",
                    QuanHuyen = n.SoNha != null && n.SoNha.PhuongXa != null && n.SoNha.PhuongXa.QuanHuyen != null ? n.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen : "",
                    TinhThanh = n.SoNha != null && n.SoNha.PhuongXa != null && n.SoNha.PhuongXa.QuanHuyen != null && n.SoNha.PhuongXa.QuanHuyen.TinhThanhPho != null ? n.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP : ""
                })
                .ToListAsync();

            if (result == null || !result.Any())
                return NotFound("Không tìm thấy người nhận nào cho mã khách hàng này");

            return Ok(result);
        }

        // POST: api/NguoiNhan/Create
        [HttpPost("Create")]
        public async Task<ActionResult> PostNguoiNhan([FromBody] NguoiNhanDTO nguoiNhanDTO)
        {
            if (nguoiNhanDTO == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            if (string.IsNullOrEmpty(nguoiNhanDTO.MaKhachHang))
                return BadRequest(new { message = "Mã khách hàng không được để trống" });

            if (string.IsNullOrEmpty(nguoiNhanDTO.HoTen) || string.IsNullOrEmpty(nguoiNhanDTO.SDT) ||
                string.IsNullOrEmpty(nguoiNhanDTO.DiaChi) || string.IsNullOrEmpty(nguoiNhanDTO.MaPhuongXa))
                return BadRequest(new { message = "Các trường thông tin bắt buộc không được để trống" });

            var khachHangExists = await _context.KhachHangs.AnyAsync(kh => kh.MaKhachHang == nguoiNhanDTO.MaKhachHang);
            if (!khachHangExists)
                return BadRequest(new { message = "Mã khách hàng không tồn tại" });

            var phuongXa = await _context.PhuongXas.FirstOrDefaultAsync(p => p.MaPhuongXa == nguoiNhanDTO.MaPhuongXa);
            if (phuongXa == null)
                return BadRequest(new { message = "Mã phường/xã không tồn tại" });

            var soNha = await _context.SoNhas.FirstOrDefaultAsync(s => s.MaSoNha == nguoiNhanDTO.MaSoNha);
            if (soNha == null)
            {
                soNha = new SoNha
                {
                    MaSoNha = nguoiNhanDTO.MaSoNha ?? $"SN{DateTime.UtcNow.Ticks}",
                    MaPhuongXa = nguoiNhanDTO.MaPhuongXa,
                    DiaChiSoNha = nguoiNhanDTO.DiaChi
                };
                _context.SoNhas.Add(soNha);
                await _context.SaveChangesAsync();
            }
            else
            {
                soNha.DiaChiSoNha = nguoiNhanDTO.DiaChi;
                soNha.MaPhuongXa = nguoiNhanDTO.MaPhuongXa;
                _context.SoNhas.Update(soNha);
            }

            var nguoiNhan = new NguoiNhan
            {
                MaKhachHang = nguoiNhanDTO.MaKhachHang,
                MaSoNha = soNha.MaSoNha,
                HoTen = nguoiNhanDTO.HoTen,
                SDT = nguoiNhanDTO.SDT
            };

            if (string.IsNullOrEmpty(nguoiNhanDTO.MaNguoiNhan))
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
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
                    while (await _context.NguoiNhans.AnyAsync(x => x.MaNguoiNhan == nguoiNhan.MaNguoiNhan))
                    {
                        nextId++;
                        nguoiNhan.MaNguoiNhan = $"NN{nextId:D3}";
                    }

                    _context.NguoiNhans.Add(nguoiNhan);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Trả về đối tượng ẩn danh để tránh vòng lặp
                    var result = new
                    {
                        nguoiNhan.MaNguoiNhan,
                        nguoiNhan.MaKhachHang,
                        nguoiNhan.MaSoNha,
                        nguoiNhan.HoTen,
                        nguoiNhan.SDT
                    };
                    return CreatedAtAction(nameof(GetNguoiNhanByMaKhachHang), new { maKhachHang = nguoiNhan.MaKhachHang }, result);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { message = "Lỗi khi tạo người nhận. Vui lòng thử lại sau!" });
                }
            }
            else
            {
                nguoiNhan.MaNguoiNhan = nguoiNhanDTO.MaNguoiNhan;
                if (await _context.NguoiNhans.AnyAsync(x => x.MaNguoiNhan == nguoiNhan.MaNguoiNhan && x.MaNguoiNhan != nguoiNhanDTO.MaNguoiNhan))
                    return Conflict(new { message = "Mã người nhận đã tồn tại" });

                _context.NguoiNhans.Update(nguoiNhan);
                await _context.SaveChangesAsync();

                var result = new
                {
                    nguoiNhan.MaNguoiNhan,
                    nguoiNhan.MaKhachHang,
                    nguoiNhan.MaSoNha,
                    nguoiNhan.HoTen,
                    nguoiNhan.SDT
                };
                return Ok(result);
            }
        }

        // PUT: api/NguoiNhan/Update
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateNguoiNhan([FromBody] NguoiNhanDTO nguoiNhanDTO)
        {
            if (nguoiNhanDTO == null || string.IsNullOrEmpty(nguoiNhanDTO.MaNguoiNhan))
                return BadRequest("Dữ liệu không hợp lệ hoặc thiếu mã người nhận");

            if (string.IsNullOrEmpty(nguoiNhanDTO.HoTen) || string.IsNullOrEmpty(nguoiNhanDTO.SDT) ||
                string.IsNullOrEmpty(nguoiNhanDTO.DiaChi) || string.IsNullOrEmpty(nguoiNhanDTO.MaPhuongXa))
                return BadRequest("Các trường thông tin bắt buộc không được để trống");

            var existingNguoiNhan = await _context.NguoiNhans
                .Include(n => n.SoNha)
                    .ThenInclude(s => s.PhuongXa)
                        .ThenInclude(p => p.QuanHuyen)
                            .ThenInclude(q => q.TinhThanhPho)
                .FirstOrDefaultAsync(n => n.MaNguoiNhan == nguoiNhanDTO.MaNguoiNhan);

            if (existingNguoiNhan == null)
                return NotFound("Không tìm thấy người nhận để cập nhật");

            // Cập nhật thông tin NguoiNhan
            existingNguoiNhan.HoTen = nguoiNhanDTO.HoTen;
            existingNguoiNhan.SDT = nguoiNhanDTO.SDT;

            // Kiểm tra và cập nhật thông tin SoNha
            var phuongXa = await _context.PhuongXas.FirstOrDefaultAsync(p => p.MaPhuongXa == nguoiNhanDTO.MaPhuongXa);
            if (phuongXa == null)
                return BadRequest("Mã phường/xã không tồn tại");

            if (existingNguoiNhan.SoNha != null)
            {
                existingNguoiNhan.SoNha.DiaChiSoNha = nguoiNhanDTO.DiaChi;
                existingNguoiNhan.SoNha.MaPhuongXa = nguoiNhanDTO.MaPhuongXa;
                _context.SoNhas.Update(existingNguoiNhan.SoNha);
            }
            else
            {
                var soNha = new SoNha
                {
                    MaSoNha = nguoiNhanDTO.MaSoNha ?? $"SN{DateTime.UtcNow.Ticks}",
                    MaPhuongXa = nguoiNhanDTO.MaPhuongXa,
                    DiaChiSoNha = nguoiNhanDTO.DiaChi
                };
                existingNguoiNhan.MaSoNha = soNha.MaSoNha;
                _context.SoNhas.Add(soNha);
            }

            _context.NguoiNhans.Update(existingNguoiNhan);
            await _context.SaveChangesAsync();

            // Trả về đối tượng ẩn danh để tránh vòng lặp
            var result = new
            {
                existingNguoiNhan.MaNguoiNhan,
                existingNguoiNhan.MaKhachHang,
                existingNguoiNhan.MaSoNha,
                existingNguoiNhan.HoTen,
                existingNguoiNhan.SDT,
                DiaChi = existingNguoiNhan.SoNha != null ? existingNguoiNhan.SoNha.DiaChiSoNha : "",
                PhuongXa = existingNguoiNhan.SoNha != null && existingNguoiNhan.SoNha.PhuongXa != null ? existingNguoiNhan.SoNha.PhuongXa.TenPhuongXa : "",
                QuanHuyen = existingNguoiNhan.SoNha != null && existingNguoiNhan.SoNha.PhuongXa != null && existingNguoiNhan.SoNha.PhuongXa.QuanHuyen != null ? existingNguoiNhan.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen : "",
                TinhThanh = existingNguoiNhan.SoNha != null && existingNguoiNhan.SoNha.PhuongXa != null && existingNguoiNhan.SoNha.PhuongXa.QuanHuyen != null && existingNguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho != null ? existingNguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP : ""
            };
            return Ok(new { message = "Cập nhật thông tin người nhận thành công", data = result });
        }

        // DELETE: api/NguoiNhan/Delete
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteNguoiNhan(string maNguoiNhan)
        {
            if (string.IsNullOrEmpty(maNguoiNhan))
                return BadRequest("Mã người nhận không hợp lệ");

            var nguoiNhan = await _context.NguoiNhans
                .Include(n => n.SoNha)
                .FirstOrDefaultAsync(n => n.MaNguoiNhan == maNguoiNhan);

            if (nguoiNhan == null)
                return NotFound("Không tìm thấy người nhận để xóa");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (nguoiNhan.SoNha != null)
                {
                    _context.SoNhas.Remove(nguoiNhan.SoNha);
                }

                _context.NguoiNhans.Remove(nguoiNhan);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Ok(new { message = "Xóa người nhận thành công" });
        }

        // DTO để nhận dữ liệu từ client
        public class NguoiNhanDTO
        {
            public string MaNguoiNhan { get; set; }
            public string MaKhachHang { get; set; }
            public string MaSoNha { get; set; }
            public string HoTen { get; set; }
            public string SDT { get; set; }
            public string DiaChi { get; set; }
            public string MaPhuongXa { get; set; }
        }
    }
}