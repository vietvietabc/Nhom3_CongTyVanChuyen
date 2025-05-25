using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NVGHController : ControllerBase
    {
        private readonly MyDbContext _context;

        public NVGHController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/NVGH/ByStatus - Lấy danh sách đơn hàng theo trạng thái và nhân viên
        [HttpGet("ByStatus")]
        public ActionResult GetByStatus(string TrangThaiDonHang, string MaNhanVien)
        {
            // Log để debug
            System.Diagnostics.Debug.WriteLine($"Getting orders with status: {TrangThaiDonHang} for employee: {MaNhanVien}");

            try
            {
                var donHangs = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Where(d => d.TrangThaiDonHang == TrangThaiDonHang && d.MaNhanVien == MaNhanVien)
                    .ToList();

                // Chuyển đổi dữ liệu sang định dạng phù hợp với frontend
                var result = donHangs.Select(d => new
                {
                    ma = d.MaDonHang,
                    maVanDon = d.MaVanDon,
                    tenKhachHang = d.KhachHang?.TenKhachHang ?? "Không có thông tin",
                    sdtKhachHang = d.KhachHang?.SDT ?? "Không có thông tin",
                    diaChiGiao = GetDiaChiGiaoHang(d.MaDonHang),
                    ngayGui = d.NgayGui,
                    trangThai = d.TrangThaiDonHang,
                    daThanhToan = d.TrangThaiThanhToan == "Đã thanh toán",
                    phuongThucThanhToan = d.PhuongThucThanhToan,
                    tienThuHo = d.TienThuHo
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Lỗi khi lấy dữ liệu đơn hàng");
            }
        }

        // GET: api/NVGH/History - Lấy lịch sử đơn hàng của nhân viên
        [HttpGet("History")]
        public ActionResult GetHistory(string MaNhanVien)
        {
            try
            {
                var donHangs = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Where(d => d.MaNhanVien == MaNhanVien &&
                           (d.TrangThaiDonHang == "Đã giao" || d.TrangThaiDonHang == "Thất bại" ||
                            d.TrangThaiDonHang == "Không nhận hàng"))
                    .ToList();

                var result = donHangs.Select(d => new
                {
                    ma = d.MaDonHang,
                    tenKhachHang = d.KhachHang?.TenKhachHang ?? "Không có thông tin",
                    ngayNhan = d.NgayNhan,
                    ngayGiao = d.NgayNhan, // Hoặc có thể tính toán ngày giao từ thông tin khác
                    trangThai = d.TrangThaiDonHang,
                    tienThuHo = d.TienThuHo
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy lịch sử đơn hàng");
            }
        }

        // GET: api/NVGH/GetAllOrders - Lấy tất cả đơn hàng của nhân viên
        [HttpGet("GetAllOrders")]
        public ActionResult GetAllOrders(string MaNhanVien)
        {
            try
            {
                // Log để debug
                System.Diagnostics.Debug.WriteLine($"Getting all orders for employee: {MaNhanVien}");

                // Kiểm tra tham số
                if (string.IsNullOrEmpty(MaNhanVien))
                {
                    return BadRequest("Thiếu mã nhân viên");
                }

                var donHangs = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Where(d => d.MaNhanVien == MaNhanVien)
                    .ToList();

                // Chuyển đổi dữ liệu sang định dạng phù hợp với frontend
                var result = donHangs.Select(d => new
                {
                    ma = d.MaDonHang,
                    maVanDon = d.MaVanDon,
                    tenKhachHang = d.KhachHang?.TenKhachHang ?? "Không có thông tin",
                    sdtKhachHang = d.KhachHang?.SDT ?? "Không có thông tin",
                    diaChiGiao = GetDiaChiGiaoHang(d.MaDonHang),
                    ngayGui = d.NgayGui,
                    trangThai = d.TrangThaiDonHang,
                    daThanhToan = d.TrangThaiThanhToan == "Đã thanh toán",
                    phuongThucThanhToan = d.PhuongThucThanhToan,
                    tienThuHo = d.TienThuHo
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllOrders: {ex.Message}");
                return StatusCode(500, "Lỗi khi lấy dữ liệu đơn hàng: " + ex.Message);
            }
        }

        // GET: api/NVGH/DeliveredOrders - Lấy các đơn hàng đã giao của nhân viên
        [HttpGet("DeliveredOrders")]
        public ActionResult GetDeliveredOrders(string MaNhanVien)
        {
            try
            {
                var donHangs = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Where(d => d.MaNhanVien == MaNhanVien && d.TrangThaiDonHang == "Đã giao")
                    .ToList();

                var result = donHangs.Select(d => new
                {
                    ma = d.MaDonHang,
                    maVanDon = d.MaVanDon,
                    tenKhachHang = d.KhachHang?.TenKhachHang ?? "Không có thông tin",
                    sdtKhachHang = d.KhachHang?.SDT ?? "Không có thông tin",
                    diaChiGiao = GetDiaChiGiaoHang(d.MaDonHang),
                    ngayGui = d.NgayGui,
                    ngayNhan = d.NgayNhan,
                    trangThai = d.TrangThaiDonHang,
                    daThanhToan = d.TrangThaiThanhToan == "Đã thanh toán",
                    trangThaiThanhToan = d.TrangThaiThanhToan,
                    phuongThucThanhToan = d.PhuongThucThanhToan ?? "Chưa có",
                    tienThuHo = d.TienThuHo,
                    ghiChu = d.GhiChu
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDeliveredOrders: {ex.Message}");
                return StatusCode(500, "Lỗi khi lấy danh sách đơn hàng đã giao: " + ex.Message);
            }
        }

        // GET: api/DonHang/{id}
        [HttpGet("{id}")]
        public ActionResult GetDonHang(string id)
        {
            try
            {
                var donHang = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .FirstOrDefault(d => d.MaDonHang == id);

                if (donHang == null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }

                var result = new
                {
                    maDonHang = donHang.MaDonHang,
                    maVanDon = donHang.MaVanDon,
                    tenKhachHang = donHang.KhachHang?.TenKhachHang ?? "Không có thông tin",
                    sdtKhachHang = donHang.KhachHang?.SDT ?? "Không có thông tin",
                    ngayGui = donHang.NgayGui,
                    ngayNhan = donHang.NgayNhan,
                    trangThaiDonHang = donHang.TrangThaiDonHang,
                    trangThaiThanhToan = donHang.TrangThaiThanhToan,
                    phuongThucThanhToan = donHang.PhuongThucThanhToan,
                    tienThuHo = donHang.TienThuHo,
                    ghiChu = donHang.GhiChu
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy thông tin đơn hàng: " + ex.Message);
            }
        }

        // GET: api/NVGH/COD - Lấy danh sách đơn hàng COD
        [HttpGet("COD")]
        public ActionResult GetCOD(string MaNhanVien)
        {
            try
            {
                var donHangs = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .Where(d => d.MaNhanVien == MaNhanVien && d.TienThuHo > 0)
                    .ToList();

                var result = donHangs.Select(d => new
                {
                    ma = d.MaDonHang,
                    tenKhachHang = d.KhachHang?.TenKhachHang ?? "Không có thông tin",
                    sdtKhachHang = d.KhachHang?.SDT ?? "Không có thông tin",
                    diaChiGiao = GetDiaChiGiaoHang(d.MaDonHang),
                    ngayGui = d.NgayGui,
                    trangThai = d.TrangThaiDonHang,
                    daThanhToan = d.TrangThaiThanhToan == "Đã thanh toán",
                    tienThuHo = d.TienThuHo,
                    trangThaiThuHo = d.TrangThaiThuHo
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy danh sách đơn COD");
            }
        }

        // GET: api/NVGH/ThuNhap - Lấy thông tin thu nhập của nhân viên từ phí giao hàng
        [HttpGet("ThuNhap")]
        public ActionResult GetThuNhap(string MaNhanVien, DateTime? TuNgay = null, DateTime? DenNgay = null)
        {
            try
            {
                // Log để debug
                System.Diagnostics.Debug.WriteLine($"Getting income for employee: {MaNhanVien} from {TuNgay} to {DenNgay}");

                // Nếu không có ngày, lấy từ đầu tháng
                if (!TuNgay.HasValue)
                {
                    TuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                if (!DenNgay.HasValue)
                {
                    DenNgay = DateTime.Now;
                }

                // Lấy tất cả đơn hàng của nhân viên
                var allDonHangs = _context.DonHangs
                    .Where(d => d.MaNhanVien == MaNhanVien)
                    .ToList();

                // Log số lượng đơn hàng để debug
                System.Diagnostics.Debug.WriteLine($"Total orders found: {allDonHangs.Count}");

                // Lọc đơn hàng đã giao
                var daGiaoDonHangs = allDonHangs
                    .Where(d => d.TrangThaiDonHang == "Đã giao")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Delivered orders: {daGiaoDonHangs.Count}");

                // Lọc đơn đã giao và đã thanh toán
                var donHangs = daGiaoDonHangs
                    .Where(d => d.TrangThaiThanhToan == "Đã thanh toán")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Delivered and paid orders: {donHangs.Count}");

                // Kiểm tra xem có đơn nào không
                if (donHangs.Count == 0)
                {
                    // Log các trạng thái của đơn để debug
                    var trangThaiDonHang = allDonHangs.Select(d => d.TrangThaiDonHang).Distinct().ToList();
                    var trangThaiThanhToan = allDonHangs.Select(d => d.TrangThaiThanhToan).Distinct().ToList();

                    System.Diagnostics.Debug.WriteLine($"Order statuses: {string.Join(", ", trangThaiDonHang)}");
                    System.Diagnostics.Debug.WriteLine($"Payment statuses: {string.Join(", ", trangThaiThanhToan)}");

                    // Lấy đơn đã giao nhưng chưa thanh toán để hiển thị
                    var donChuaThanhToan = daGiaoDonHangs
                        .Where(d => d.TrangThaiThanhToan != "Đã thanh toán")
                        .Select(d => new
                        {
                            ma = d.MaDonHang,
                            ngayGiao = d.NgayNhan,
                            trangThai = d.TrangThaiDonHang,
                            trangThaiThanhToan = d.TrangThaiThanhToan,
                            phiGiaoHang = d.PhiGiaoHang
                        })
                        .ToList();

                    return Ok(new
                    {
                        tongThuNhap = 0.0,
                        soDonHangDaGiao = daGiaoDonHangs.Count,
                        donHangGanDay = new List<object>(),
                        thuNhapTheoNgay = new List<object>(),
                        donChuaThanhToan, // Đơn chưa thanh toán
                        thongBao = "Không có đơn hàng nào đã giao và đã thanh toán"
                    });
                }

                // Tính tổng thu nhập và thông tin chi tiết
                double tongThuNhap = donHangs.Sum(d => d.PhiGiaoHang);
                int soDonHangDaGiao = donHangs.Count;

                // Kiểm tra giá trị PhiGiaoHang
                foreach (var donHang in donHangs.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"Order {donHang.MaDonHang} fee: {donHang.PhiGiaoHang}");
                }

                // Lấy 10 đơn gần đây nhất để hiển thị
                var donHangGanDay = donHangs
                    .OrderByDescending(d => d.NgayNhan)
                    .Take(10)
                    .Select(d => new
                    {
                        ma = d.MaDonHang,
                        ngayGiao = d.NgayNhan,
                        phiGiaoHang = d.PhiGiaoHang
                    })
                    .ToList();

                // Tính thu nhập theo ngày gần đây (7 ngày)
                var thuNhapTheoNgay = donHangs
                    .Where(d => d.NgayNhan.HasValue && d.NgayNhan >= DateTime.Now.AddDays(-7))
                    .GroupBy(d => d.NgayNhan.Value.Date)
                    .Select(g => new
                    {
                        ngay = g.Key.ToString("yyyy-MM-dd"),
                        thuNhap = g.Sum(d => d.PhiGiaoHang),
                        soDon = g.Count()
                    })
                    .OrderBy(d => d.ngay)
                    .ToList();

                var result = new
                {
                    tongThuNhap,
                    soDonHangDaGiao,
                    donHangGanDay,
                    thuNhapTheoNgay
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetThuNhap: {ex.Message}");
                return StatusCode(500, "Lỗi khi lấy thông tin thu nhập: " + ex.Message);
            }
        }

        // GET: api/NVGH/KiemTraDuLieu - API để kiểm tra dữ liệu đơn hàng và debug
        [HttpGet("KiemTraDuLieu")]
        public ActionResult KiemTraDuLieu(string MaNhanVien)
        {
            try
            {
                // Lấy tất cả đơn hàng của nhân viên
                var allOrders = _context.DonHangs
                    .Where(d => d.MaNhanVien == MaNhanVien)
                    .Select(d => new
                    {
                        d.MaDonHang,
                        d.TrangThaiDonHang,
                        d.TrangThaiThanhToan,
                        d.PhiGiaoHang,
                        d.NgayGui,
                        d.NgayNhan
                    })
                    .ToList();

                // Đếm theo các trạng thái
                var countByOrderStatus = allOrders
                    .GroupBy(d => d.TrangThaiDonHang)
                    .Select(g => new
                    {
                        TrangThai = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();

                var countByPaymentStatus = allOrders
                    .GroupBy(d => d.TrangThaiThanhToan)
                    .Select(g => new
                    {
                        TrangThai = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();

                // Kiểm tra giá trị PhiGiaoHang
                var countByFeeValue = allOrders
                    .GroupBy(d => d.PhiGiaoHang > 0)
                    .Select(g => new
                    {
                        CoPhiGiaoHang = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();

                return Ok(new
                {
                    TongDon = allOrders.Count,
                    PhanLoaiTrangThaiDon = countByOrderStatus,
                    PhanLoaiTrangThaiThanhToan = countByPaymentStatus,
                    ThongKePhiGiaoHang = countByFeeValue,
                    DanhSachDon = allOrders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi kiểm tra dữ liệu: " + ex.Message);
            }
        }

        [HttpPost("UpdateStatus")]
        public ActionResult UpdateStatus([FromBody] UpdateStatusModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Dữ liệu không hợp lệ");

                if (string.IsNullOrEmpty(model.MaDonHang))
                    return BadRequest("Thiếu mã đơn hàng");

                if (string.IsNullOrEmpty(model.TrangThaiMoi))
                    return BadRequest("Thiếu trạng thái mới");

                var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == model.MaDonHang);
                if (donHang == null)
                    return NotFound($"Không tìm thấy đơn hàng với mã {model.MaDonHang}");

                var trangThaiMoi = model.TrangThaiMoi.Trim();

                // Cập nhật trạng thái đơn hàng
                donHang.TrangThaiDonHang = trangThaiMoi;

                // Nếu là trạng thái "đã giao" thì cập nhật ngày nhận
                if (trangThaiMoi.Equals("đã giao", StringComparison.OrdinalIgnoreCase))
                {
                    donHang.NgayNhan = DateTime.Now;
                }

                // Reset thông tin thanh toán nếu các trạng thái đặc biệt
                if (trangThaiMoi.Equals("khách hẹn lại ngày giao", StringComparison.OrdinalIgnoreCase)
                    || trangThaiMoi.Equals("đang giao", StringComparison.OrdinalIgnoreCase)
                    || trangThaiMoi.Equals("khách không nhận hàng", StringComparison.OrdinalIgnoreCase))
                {
                    donHang.TrangThaiThanhToan = "chưa thanh toán";
                    donHang.NgayThanhToan = null;
                    donHang.PhuongThucThanhToan = null;
                    donHang.TrangThaiThuHo = null;
                }

                // Cập nhật ghi chú nếu có
                if (!string.IsNullOrEmpty(model.GhiChu))
                {
                    donHang.GhiChu = model.GhiChu;
                }

                _context.SaveChanges();

                return Ok(new
                {
                    message = "Cập nhật trạng thái đơn hàng thành công",
                    maDonHang = donHang.MaDonHang,
                    trangThaiMoi = donHang.TrangThaiDonHang
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}");
            }
        }

        public class UpdateStatusModel
        {
            public string MaDonHang { get; set; }
            public string TrangThaiMoi { get; set; }
            public string GhiChu { get; set; }
        }


        // POST: api/NVGH/UpdatePayment - Cập nhật trạng thái thanh toán
        [HttpPost("UpdatePayment")]
        public ActionResult UpdatePayment([FromBody] UpdatePaymentModel model)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDonHang == model.MaDonHang);
                if (donHang == null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }

                // Lưu trạng thái thanh toán cũ để ghi log
                var trangThaiCu = donHang.TrangThaiThanhToan;

                // Cập nhật thông tin thanh toán
                donHang.TrangThaiThanhToan = model.TrangThaiThanhToan;
                donHang.PhuongThucThanhToan = model.PhuongThucThanhToan;

                // Chỉ cập nhật ngày thanh toán khi trạng thái là "Đã thanh toán"
                if (model.TrangThaiThanhToan == "Đã thanh toán")
                {
                    donHang.NgayThanhToan = DateTime.Now;
                }
                else
                {
                    donHang.NgayThanhToan = null;
                }
                if (!string.IsNullOrEmpty(model.GhiChu))
                {
                    if (string.IsNullOrEmpty(donHang.GhiChu))
                        donHang.GhiChu = model.GhiChu;
                    else
                        donHang.GhiChu += $"\n{model.GhiChu}";
                }

                _context.SaveChanges();

                return Ok(new
                {
                    message = "Cập nhật thanh toán thành công",
                    trangThaiCu = trangThaiCu,
                    trangThaiMoi = model.TrangThaiThanhToan
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi cập nhật thanh toán: " + ex.Message);
            }
        }
        // Method hỗ trợ để lấy địa chỉ giao hàng
        private string GetDiaChiGiaoHang(string maDonHang)
        {
            // Logic để lấy địa chỉ giao hàng, có thể từ bảng NguoiNhan hoặc trực tiếp từ KhachHang
            try
            {
                // Cố gắng lấy từ NguoiNhan (nếu có liên kết DonHang-NguoiNhan)
                var nguoiNhan = _context.DonHangs
                    .Where(d => d.MaDonHang == maDonHang)
                    .Join(_context.Set<NguoiNhan>(),
                          d => d.MaKhachHang,
                          n => n.MaKhachHang,
                          (d, n) => n)
                    .FirstOrDefault();

                if (nguoiNhan != null)
                {
                    var soNha = _context.Set<SoNha>()
                        .FirstOrDefault(s => s.MaSoNha == nguoiNhan.MaSoNha);

                    return soNha?.MaSoNha ?? "Chưa có địa chỉ";
                }

                // Nếu không tìm thấy, lấy từ KhachHang
                var donHang = _context.DonHangs
                    .Include(d => d.KhachHang)
                    .FirstOrDefault(d => d.MaDonHang == maDonHang);

                return donHang?.KhachHang?.MaSoNha ?? "Chưa có địa chỉ";
            }
            catch
            {
                return "Không tìm thấy địa chỉ";
            }
        }
    }

    // Model cho việc cập nhật trạng thái
    public class UpdateStatusModel
    {
        public string MaDonHang { get; set; }
        public string TrangThaiMoi { get; set; }
        public string GhiChu { get; set; }
    }

    // Model cho việc cập nhật thanh toán
public class UpdatePaymentModel
{
    public string MaDonHang { get; set; }
    public string TrangThaiThanhToan { get; set; }
    public string PhuongThucThanhToan { get; set; }
    public string GhiChu { get; set; }
}
}