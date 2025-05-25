using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController2 : ControllerBase
    {
        private readonly MyDbContext _context;

        public DonHangController2(MyDbContext context)
        {
            _context = context;
        }

        // POST: api/DonHangController2/Create
        [HttpPost("Create")]
        public async Task<ActionResult> CreateDonHang([FromBody] DonHangCreateDTO donHangDTO)
        {
            try
            {
                if (donHangDTO == null)
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });

                // Validate required fields
                if (string.IsNullOrEmpty(donHangDTO.MaKhachHang) ||
                    string.IsNullOrEmpty(donHangDTO.MaNguoiNhan) ||
                    string.IsNullOrEmpty(donHangDTO.TenDonHang))
                {
                    return BadRequest(new { message = "Thiếu thông tin bắt buộc" });
                }

                // Check if customer exists
                var khachHang = await _context.KhachHangs.FindAsync(donHangDTO.MaKhachHang);
                if (khachHang == null)
                    return BadRequest(new { message = "Khách hàng không tồn tại" });

                // Check if recipient exists
                var nguoiNhan = await _context.NguoiNhans.FindAsync(donHangDTO.MaNguoiNhan);
                if (nguoiNhan == null)
                    return BadRequest(new { message = "Người nhận không tồn tại" });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Generate order ID
                    var lastOrder = await _context.DonHangs
                        .OrderByDescending(d => d.MaDonHang)
                        .FirstOrDefaultAsync();

                    int nextId = 1;
                    if (lastOrder != null && lastOrder.MaDonHang.StartsWith("DH"))
                    {
                        if (int.TryParse(lastOrder.MaDonHang.Substring(2), out int lastId))
                            nextId = lastId + 1;
                    }

                    string maDonHang = $"DH{nextId:D6}";
                    while (await _context.DonHangs.AnyAsync(x => x.MaDonHang == maDonHang))
                    {
                        nextId++;
                        maDonHang = $"DH{nextId:D6}";
                    }

                    // Create order
                    var donHang = new DonHang
                    {
                        MaDonHang = maDonHang,
                        MaKhachHang = donHangDTO.MaKhachHang,
                        MaNguoiNhan = donHangDTO.MaNguoiNhan,
                        TenDonHang = donHangDTO.TenDonHang, // This comes from packageName in frontend
                        PhiGiaoHang = donHangDTO.PhiGiaoHang, // Sử dụng phí từ frontend
                        TienThuHo = donHangDTO.TienThuHo,
                        NguoiTraPhi = donHangDTO.NguoiTraPhi,
                        NgayGui = DateTime.Now,
                        TrangThaiDonHang = "chờ duyệt",
                        TrangThaiThanhToan = "Chưa thanh toán",
                        TrangThaiThuHo = donHangDTO.TienThuHo > 0 ? "Chưa thu" : "Không áp dụng",
                        GhiChu = donHangDTO.GhiChu
                    };

                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();

                    // Create order details if provided
                    if (donHangDTO.ChiTietDonHang != null)
                    {
                        // Find or create HangHoa based on selected characteristics and category
                        var hangHoa = await FindOrCreateHangHoa(donHangDTO.ChiTietDonHang);

                        // Generate detail ID
                        var lastDetail = await _context.ChiTietDonHangs
                            .OrderByDescending(c => c.MaChiTietDonHang)
                            .FirstOrDefaultAsync();

                        int nextDetailId = 1;
                        if (lastDetail != null && lastDetail.MaChiTietDonHang.StartsWith("CT"))
                        {
                            if (int.TryParse(lastDetail.MaChiTietDonHang.Substring(2), out int lastDetailId))
                                nextDetailId = lastDetailId + 1;
                        }

                        string maChiTiet = $"CT{nextDetailId:D6}";
                        while (await _context.ChiTietDonHangs.AnyAsync(x => x.MaChiTietDonHang == maChiTiet))
                        {
                            nextDetailId++;
                            maChiTiet = $"CT{nextDetailId:D6}";
                        }

                        var chiTiet = new ChiTietDonHang
                        {
                            MaChiTietDonHang = maChiTiet, // Auto-generated
                            MaDonHang = maDonHang, // Links to the order
                            MaHangHoa = hangHoa.MaHangHoa,
                            SoLuong = donHangDTO.ChiTietDonHang.SoLuong, // From quantity field
                            TrongLuong = donHangDTO.ChiTietDonHang.TrongLuong, // From weight field
                            KichThuoc = donHangDTO.ChiTietDonHang.KichThuoc // From dimensions field
                        };

                        _context.ChiTietDonHangs.Add(chiTiet);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetDonHang), new { id = maDonHang }, new
                    {
                        maDonHang = donHang.MaDonHang,
                        message = "Tạo đơn hàng thành công"
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo đơn hàng: " + ex.Message });
            }
        }

        // Helper method to find or create HangHoa
        private async Task<HangHoa> FindOrCreateHangHoa(ChiTietDonHangDTO chiTietDTO)
        {
            // Use the MaDanhMuc directly from the form (selected from dropdown)
            var maDanhMuc = chiTietDTO.LoaiHangHoa; // This now contains the actual MaDanhMuc from DanhMucs table

            // Try to find existing HangHoa with same characteristics
            var existingHangHoa = await _context.HangHoas
                .Where(hh => hh.MaDanhMuc == maDanhMuc &&
                           hh.TinhChatHangHoa == chiTietDTO.TinhChatDacBiet)
                .FirstOrDefaultAsync();

            if (existingHangHoa != null)
            {
                return existingHangHoa;
            }

            // Create new HangHoa
            var lastProduct = await _context.HangHoas
                .OrderByDescending(hh => hh.MaHangHoa)
                .FirstOrDefaultAsync();

            int nextId = 1;
            if (lastProduct != null && lastProduct.MaHangHoa.StartsWith("HH"))
            {
                if (int.TryParse(lastProduct.MaHangHoa.Substring(2), out int lastId))
                    nextId = lastId + 1;
            }

            string maHangHoa = $"HH{nextId:D3}";
            while (await _context.HangHoas.AnyAsync(x => x.MaHangHoa == maHangHoa))
            {
                nextId++;
                maHangHoa = $"HH{nextId:D3}";
            }

            var newHangHoa = new HangHoa
            {
                MaHangHoa = maHangHoa,
                MaDanhMuc = maDanhMuc,
                TinhChatHangHoa = chiTietDTO.TinhChatDacBiet,
                DonGia = 0 // Default price, can be updated later
            };

            _context.HangHoas.Add(newHangHoa);
            await _context.SaveChangesAsync();

            return newHangHoa;
        }

        // GET: api/DonHangController2/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDonHang(string id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Include(d => d.NguoiNhan)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.HangHoa)
                            .ThenInclude(hh => hh.DanhMuc)
                    .FirstOrDefaultAsync(d => d.MaDonHang == id);

                if (donHang == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                return Ok(donHang);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin đơn hàng: " + ex.Message });
            }
        }

        // GET: api/DonHangController2/GetByKhachHang/{maKhachHang}
        [HttpGet("GetByKhachHang/{maKhachHang}")]
        public async Task<ActionResult> GetDonHangByKhachHang(string maKhachHang)
        {
            try
            {
                var donHangs = await _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang)
                    .Include(d => d.NguoiNhan)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.HangHoa)
                    .OrderByDescending(d => d.NgayGui)
                    .ToListAsync();

                return Ok(donHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng: " + ex.Message });
            }
        }

        // PUT: api/DonHangController2/UpdateStatus/{id}
        [HttpPut("UpdateStatus/{id}")]
        public async Task<ActionResult> UpdateOrderStatus(string id, [FromBody] UpdateStatusDTO statusDTO)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(id);
                if (donHang == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                if (!string.IsNullOrEmpty(statusDTO.TrangThaiDonHang))
                    donHang.TrangThaiDonHang = statusDTO.TrangThaiDonHang;

                if (!string.IsNullOrEmpty(statusDTO.TrangThaiThanhToan))
                    donHang.TrangThaiThanhToan = statusDTO.TrangThaiThanhToan;

                if (!string.IsNullOrEmpty(statusDTO.TrangThaiThuHo))
                    donHang.TrangThaiThuHo = statusDTO.TrangThaiThuHo;

                if (statusDTO.NgayNhan.HasValue)
                    donHang.NgayNhan = statusDTO.NgayNhan.Value;

                if (statusDTO.NgayThanhToan.HasValue)
                    donHang.NgayThanhToan = statusDTO.NgayThanhToan.Value;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái: " + ex.Message });
            }
        }

        // DELETE: api/DonHangController2/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDonHang(string id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .FirstOrDefaultAsync(d => d.MaDonHang == id);

                if (donHang == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                // Check if order can be deleted (only allow deletion of pending orders)
                if (donHang.TrangThaiDonHang != "chờ duyệt")
                    return BadRequest(new { message = "Chỉ có thể xóa đơn hàng đang chờ xử lý" });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Delete order details first
                    if (donHang.ChiTietDonHangs != null && donHang.ChiTietDonHangs.Any())
                    {
                        _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);
                    }

                    // Delete order
                    _context.DonHangs.Remove(donHang);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { message = "Xóa đơn hàng thành công" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa đơn hàng: " + ex.Message });
            }
        }

        // DTO classes
        public class DonHangCreateDTO
        {
            public string MaKhachHang { get; set; }
            public string MaNguoiNhan { get; set; }
            public string TenDonHang { get; set; }
            public double PhiGiaoHang { get; set; } // Thêm dòng này
            public double TienThuHo { get; set; }
            public string NguoiTraPhi { get; set; }
            public string GhiChu { get; set; }
            public ChiTietDonHangDTO ChiTietDonHang { get; set; }
        }

        public class ChiTietDonHangDTO
        {
            public string LoaiHangHoa { get; set; } // This will contain MaDanhMuc
            public string TenHang { get; set; }
            public int SoLuong { get; set; }
            public double TrongLuong { get; set; }
            public string KichThuoc { get; set; }
            public string TinhChatDacBiet { get; set; }
        }

        public class UpdateStatusDTO
        {
            public string TrangThaiDonHang { get; set; }
            public string TrangThaiThanhToan { get; set; }
            public string TrangThaiThuHo { get; set; }
            public DateTime? NgayNhan { get; set; }
            public DateTime? NgayThanhToan { get; set; }
        }
    }
}
