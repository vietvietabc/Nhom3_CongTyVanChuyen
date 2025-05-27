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
                        TrangThaiDonHang = "Chờ duyệt",
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
                    .OrderByDescending(d => d.NgayGui)
                    .Select(d => new
                    {
                        d.MaDonHang,
                        d.MaVanDon,
                        d.TenDonHang,
                        d.PhiGiaoHang,
                        d.TienThuHo,
                        d.NguoiTraPhi,
                        d.NgayGui,
                        d.NgayNhan,
                        d.NgayThanhToan,
                        d.TrangThaiDonHang,
                        d.TrangThaiThanhToan,
                        d.TrangThaiThuHo,
                        d.GhiChu,
                        // Only include necessary fields from related entities
                        NguoiNhan = d.NguoiNhan != null ? new
                        {
                            d.NguoiNhan.HoTen,
                            d.NguoiNhan.SDT
                        } : null,
                        ChiTietDonHangs = d.ChiTietDonHangs.Select(ct => new
                        {
                            ct.MaChiTietDonHang,
                            ct.SoLuong,
                            ct.TrongLuong,
                            ct.KichThuoc,
                            HangHoa = ct.HangHoa != null ? new
                            {
                                ct.HangHoa.MaHangHoa,
                                ct.HangHoa.TinhChatHangHoa,
                                ct.HangHoa.DonGia // Include DonGia for calculation
                            } : null
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(donHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng: " + ex.Message });
            }
        }

        // GET: api/DonHangController2/GetOrdersForManagement/{maKhachHang}
        [HttpGet("GetOrdersForManagement/{maKhachHang}")]
        public async Task<ActionResult> GetOrdersForManagement(string maKhachHang, string search = "", string status = "")
        {
            try
            {
                var query = _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang)
                    .Include(d => d.NguoiNhan)
                    .Include(d => d.ChiTietDonHangs)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(d =>
                        (d.MaVanDon != null && d.MaVanDon.Contains(search)) ||
                        (d.NguoiNhan != null && d.NguoiNhan.HoTen.Contains(search))
                    );
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    query = query.Where(d => d.TrangThaiDonHang == status);
                }

                var orders = await query
                    .OrderByDescending(d => d.NgayGui)
                    .Select(d => new
                    {
                        d.MaDonHang,
                        d.MaVanDon,
                        d.TenDonHang,
                        SoLuong = d.ChiTietDonHangs.Sum(ct => ct.SoLuong),
                        TrongLuong = d.ChiTietDonHangs.Sum(ct => ct.TrongLuong),
                        KichThuoc = string.Join(", ", d.ChiTietDonHangs.Select(ct => ct.KichThuoc).Where(kt => !string.IsNullOrEmpty(kt))),
                        d.PhiGiaoHang,
                        d.TienThuHo,
                        d.TrangThaiDonHang,
                        d.NguoiTraPhi,
                        TenNguoiNhan = d.NguoiNhan != null ? d.NguoiNhan.HoTen : "N/A",
                        d.NgayGui
                    })
                    .ToListAsync();

                return Ok(orders);
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
                if (donHang.TrangThaiDonHang != "Chờ duyệt")
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

        // GET: api/DonHangController2/GetDashboardStats/{maKhachHang}
        [HttpGet("GetDashboardStats/{maKhachHang}")]
        public async Task<ActionResult> GetDashboardStats(string maKhachHang)
        {
            try
            {
                var donHangs = await _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang)
                    .ToListAsync();

                // Tổng đơn hàng (Đã giao, Đang giao, Khách hẹn lại ngày giao)
                var tongDonHang = donHangs.Count(d =>
                    d.TrangThaiDonHang == "Đã giao" ||
                    d.TrangThaiDonHang == "Đang giao" ||
                    d.TrangThaiDonHang == "Khách hẹn lại ngày giao");

                // Giao thành công (Đã giao)
                var giaoThanhCong = donHangs.Count(d => d.TrangThaiDonHang == "Đã giao");

                // Đơn hoàn (Khách không nhận hàng)
                var donHoan = donHangs.Count(d => d.TrangThaiDonHang == "Khách không nhận hàng");

                // Doanh thu (Tổng tiền thu hộ của đơn đã giao)
                var doanhThu = donHangs
                    .Where(d => d.TrangThaiDonHang == "Đã giao")
                    .Sum(d => d.TienThuHo);

                // Thống kê theo tháng trước để tính phần trăm thay đổi
                var thangTruoc = DateTime.Now.AddMonths(-1);
                var donHangThangTruoc = await _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang &&
                       d.NgayGui.Month == thangTruoc.Month &&
                       d.NgayGui.Year == thangTruoc.Year)
                    .ToListAsync();

                var tongDonHangThangTruoc = donHangThangTruoc.Count(d =>
                    d.TrangThaiDonHang == "Đã giao" ||
                    d.TrangThaiDonHang == "Đang giao" ||
                    d.TrangThaiDonHang == "Khách hẹn lại ngày giao");

                var giaoThanhCongThangTruoc = donHangThangTruoc.Count(d => d.TrangThaiDonHang == "Đã giao");
                var donHoanThangTruoc = donHangThangTruoc.Count(d => d.TrangThaiDonHang == "Khách không nhận hàng");
                var doanhThuThangTruoc = donHangThangTruoc
                    .Where(d => d.TrangThaiDonHang == "Đã giao")
                    .Sum(d => d.TienThuHo);

                // Tính phần trăm thay đổi
                var phanTramTongDon = tongDonHangThangTruoc > 0 ?
                    Math.Round(((double)(tongDonHang - tongDonHangThangTruoc) / tongDonHangThangTruoc) * 100, 1) : 0;

                var phanTramGiaoTC = giaoThanhCongThangTruoc > 0 ?
                    Math.Round(((double)(giaoThanhCong - giaoThanhCongThangTruoc) / giaoThanhCongThangTruoc) * 100, 1) : 0;

                var phanTramDonHoan = donHoanThangTruoc > 0 ?
                    Math.Round(((double)(donHoan - donHoanThangTruoc) / donHoanThangTruoc) * 100, 1) : 0;

                var phanTramDoanhThu = doanhThuThangTruoc > 0 ?
                    Math.Round(((doanhThu - doanhThuThangTruoc) / doanhThuThangTruoc) * 100, 1) : 0;

                var result = new
                {
                    tongDonHang = tongDonHang,
                    giaoThanhCong = giaoThanhCong,
                    donHoan = donHoan,
                    doanhThu = doanhThu,
                    phanTramThayDoi = new
                    {
                        tongDonHang = phanTramTongDon,
                        giaoThanhCong = phanTramGiaoTC,
                        donHoan = phanTramDonHoan,
                        doanhThu = phanTramDoanhThu
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê dashboard: " + ex.Message });
            }
        }

        // GET: api/DonHangController2/GetChartData/{maKhachHang}
        [HttpGet("GetChartData/{maKhachHang}")]
        public async Task<ActionResult> GetChartData(string maKhachHang, string period = "week")
        {
            try
            {
                var now = DateTime.Now;
                DateTime startDate;
                List<string> labels;
                int periodCount;

                if (period.ToLower() == "week")
                {
                    // Lấy dữ liệu 7 ngày gần nhất
                    startDate = now.AddDays(-6).Date;
                    periodCount = 7;
                    labels = new List<string>();
                    for (int i = 0; i < 7; i++)
                    {
                        var date = startDate.AddDays(i);
                        labels.Add(date.ToString("dd/MM"));
                    }
                }
                else // month
                {
                    // Lấy dữ liệu 4 tuần gần nhất
                    startDate = now.AddDays(-27).Date;
                    periodCount = 4;
                    labels = new List<string> { "Tuần 1", "Tuần 2", "Tuần 3", "Tuần 4" };
                }

                var donHangs = await _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang && d.NgayGui >= startDate)
                    .ToListAsync();

                var successData = new List<int>();
                var returnData = new List<int>();

                if (period.ToLower() == "week")
                {
                    // Thống kê theo ngày
                    for (int i = 0; i < 7; i++)
                    {
                        var date = startDate.AddDays(i);
                        var dayOrders = donHangs.Where(d => d.NgayGui.Date == date).ToList();

                        successData.Add(dayOrders.Count(d => d.TrangThaiDonHang == "Đã giao"));
                        returnData.Add(dayOrders.Count(d => d.TrangThaiDonHang == "Khách không nhận hàng"));
                    }
                }
                else
                {
                    // Thống kê theo tuần
                    for (int i = 0; i < 4; i++)
                    {
                        var weekStart = startDate.AddDays(i * 7);
                        var weekEnd = weekStart.AddDays(6);
                        var weekOrders = donHangs.Where(d => d.NgayGui.Date >= weekStart && d.NgayGui.Date <= weekEnd).ToList();

                        successData.Add(weekOrders.Count(d => d.TrangThaiDonHang == "Đã giao"));
                        returnData.Add(weekOrders.Count(d => d.TrangThaiDonHang == "Khách không nhận hàng"));
                    }
                }

                var result = new
                {
                    labels = labels,
                    successData = successData,
                    returnData = returnData,
                    period = period
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu biểu đồ: " + ex.Message });
            }
        }

        // GET: api/DonHangController2/GetRecentOrders/{maKhachHang}
        [HttpGet("GetRecentOrders/{maKhachHang}")]
        public async Task<ActionResult> GetRecentOrders(string maKhachHang)
        {
            try
            {
                // Lấy đơn hàng trong 1 tuần qua
                var oneWeekAgo = DateTime.Now.AddDays(-7);

                var recentOrders = await _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang && d.NgayGui >= oneWeekAgo)
                    .OrderByDescending(d => d.NgayGui)
                    .Select(d => new
                    {
                        d.MaDonHang,
                        d.MaVanDon,
                        d.TenDonHang,
                        d.TrangThaiDonHang,
                        d.GhiChu,
                        d.NgayGui
                    })
                    .ToListAsync();

                return Ok(recentOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy đơn hàng gần đây: " + ex.Message });
            }
        }

        // GET: api/DonHangController2/GetRevenueStats/{maKhachHang}
        [HttpGet("GetRevenueStats/{maKhachHang}")]
        public async Task<ActionResult> GetRevenueStats(string maKhachHang, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.DonHangs
                    .Where(d => d.MaKhachHang == maKhachHang);

                // Apply date filter if provided
                if (startDate.HasValue)
                    query = query.Where(d => d.NgayGui >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(d => d.NgayGui <= endDate.Value);

                var donHangs = await query.ToListAsync();

                // Calculate statistics by specific status requested
                var statusStats = new List<object>();
                var targetStatuses = new[]
                {
                    "Đã giao",
                    "Khách hẹn lại ngày giao",
                    "Khách không nhận hàng",
                    "Đang giao"
                };

                foreach (var status in targetStatuses)
                {
                    var statusOrders = donHangs.Where(d => d.TrangThaiDonHang == status).ToList();
                    statusStats.Add(new
                    {
                        trangThai = status,
                        soDon = statusOrders.Count,
                        tienThuHo = statusOrders.Sum(d => d.TienThuHo),
                        tienCuoc = statusOrders.Sum(d => d.PhiGiaoHang)
                    });
                }

                var result = new
                {
                    tongSoDon = donHangs.Count,
                    tongTienThuHo = donHangs.Sum(d => d.TienThuHo),
                    tongTienCuoc = donHangs.Sum(d => d.PhiGiaoHang),
                    chiTietTheoTrangThai = statusStats
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê doanh thu: " + ex.Message });
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
