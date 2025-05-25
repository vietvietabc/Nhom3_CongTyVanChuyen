using Microsoft.AspNetCore.Mvc;
using Nhom3_CongTyVanChuyen.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Dtos;
using System.Text.Json;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly MyDbContext _context;

        public DonHangController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDonHangs([FromQuery] string? excludeTrangThai = null)
        {
            var query = _context.DonHangs
                .Include(dh => dh.KhachHang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(excludeTrangThai))
            {
                var excludeLower = excludeTrangThai.ToLower();
                query = query.Where(dh => dh.TrangThaiDonHang.ToLower() != excludeLower);
            }

            var result = await query
                .OrderByDescending(dh => dh.NgayGui)
                .Select(dh => new
                {
                    dh.MaDonHang,
                    dh.TenDonHang,
                    dh.NgayGui,
                    dh.TrangThaiDonHang,
                    TenKhachHang = dh.KhachHang.TenKhachHang
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPut("{maDonHang}/Huy")]
        public async Task<IActionResult> HuyDonHang(string maDonHang, [FromQuery] string maNhanVien)
        {
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            if (donHang == null)
                return NotFound("Không tìm thấy đơn hàng.");

            // Lấy mã nhân viên giao hàng từ query (hoặc từ token đăng nhập)
            if (string.IsNullOrEmpty(maNhanVien))
                return BadRequest("Thiếu mã nhân viên.");

            // Tính số đơn đã hủy trong tuần này
            var startOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + 1); // Thứ 2 đầu tuần
            var endOfWeek = startOfWeek.AddDays(7);

            var huyTrongTuan = await _context.DonHangs
                .Where(dh =>
                    dh.MaNhanVien == maNhanVien &&
                    dh.TrangThaiDonHang.ToLower() == "không tiếp nhận" &&
                    dh.NgayGui >= startOfWeek && dh.NgayGui < endOfWeek
                )
                .CountAsync();

            if (huyTrongTuan >= 3)
                return BadRequest("Bạn chỉ được hủy tối đa 3 đơn mỗi tuần.");

            // Không cho hủy nếu đã giao
            if (donHang.TrangThaiDonHang != null && donHang.TrangThaiDonHang.Trim().ToLower() == "đã giao")
                return BadRequest("Đơn hàng đã giao không thể hủy.");

            donHang.TrangThaiDonHang = "không tiếp nhận";
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đơn hàng {maDonHang} đã được chuyển sang trạng thái 'không tiếp nhận'." });
        }

        [HttpGet("SoLanHuyConLai")]
        public async Task<IActionResult> GetSoLanHuyConLai([FromQuery] string maNhanVien)
        {
            if (string.IsNullOrEmpty(maNhanVien))
                return BadRequest("Thiếu mã nhân viên.");
            var startOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + 1); // Thứ 2 đầu tuần
            var endOfWeek = startOfWeek.AddDays(7);

            var huyTrongTuan = await _context.DonHangs
                .Where(dh =>
                    dh.MaNhanVien == maNhanVien &&
                    dh.TrangThaiDonHang.ToLower() == "không tiếp nhận" &&
                    dh.NgayGui >= startOfWeek && dh.NgayGui < endOfWeek
                )
                .CountAsync();

            int soLanConLai = 3 - huyTrongTuan;
            if (soLanConLai < 0) soLanConLai = 0;

            return Ok(new { soLanConLai });
        }

        [HttpGet("choduyet")]
        public async Task<IActionResult> GetDonHangsChuaDuyet()
        {
            var donHangsRaw = await _context.DonHangs
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.SoNha)
                        .ThenInclude(sn => sn.PhuongXa)
                            .ThenInclude(px => px.QuanHuyen)
                                .ThenInclude(qh => qh.TinhThanhPho)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.NguoiNhans)
                        .ThenInclude(nn => nn.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
                .Where(dh => dh.TrangThaiDonHang == "chờ duyệt")
                .ToListAsync(); // chuyển về LINQ to Objects

            var donHangs = donHangsRaw.Select(dh =>
            {
                var nguoiNhan = dh.KhachHang.NguoiNhans.FirstOrDefault();

                string diaChiNguoiNhan = "Không xác định";
                if (nguoiNhan?.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiNguoiNhan = nguoiNhan.SoNha.DiaChiSoNha + ", " +
                                      nguoiNhan.SoNha.PhuongXa.TenPhuongXa + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                return new
                {
                    maDonHang = dh.MaDonHang,
                    maVanDon = dh.MaVanDon,
                    tenDonHang = dh.TenDonHang,
                    tenKhachHang = dh.KhachHang.TenKhachHang,
                    sdtKhachHang = dh.KhachHang.SDT,
                    diaChiKhachHang = dh.KhachHang.SoNha.DiaChiSoNha + ", " +
                                      dh.KhachHang.SoNha.PhuongXa.TenPhuongXa + ", " +
                                      dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                      dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP,
                    hoTenNguoiNhan = nguoiNhan?.HoTen,
                    sdtNguoiNhan = nguoiNhan?.SDT,
                    diaChiNguoiNhan = diaChiNguoiNhan,
                    NguoiTraPhi = dh.NguoiTraPhi,
                    phiGiaoHang = dh.PhiGiaoHang,
                    ngayGui = dh.NgayGui,
                    trangThaiDonHang = dh.TrangThaiDonHang
                };
            }).ToList();

            return Ok(donHangs);
        }


        [HttpDelete("xoa/{maDonHang}")]
        public async Task<IActionResult> XoaDonHang(string maDonHang)
        {
            var donHang = await _context.DonHangs
                .Include(dh => dh.KhachHang)
                .Include(dh => dh.NguoiNhan)
                .ThenInclude(nn => nn.SoNha)
                .Include(dh => dh.KhachHang.SoNha)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (donHang == null)
                return NotFound("Đơn hàng không tồn tại.");

            // Xóa chi tiết đơn hàng nếu có
            if (donHang.ChiTietDonHangs != null)
            {
                _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);
            }

            // Xóa đơn hàng
            _context.DonHangs.Remove(donHang);

            // Xóa người nhận nếu không còn đơn hàng khác tham chiếu
            if (donHang.NguoiNhan != null)
            {
                var conDonHangNguoiNhanKhac = await _context.DonHangs.AnyAsync(dh => dh.MaNguoiNhan == donHang.MaNguoiNhan && dh.MaDonHang != maDonHang);
                if (!conDonHangNguoiNhanKhac)
                {
                    _context.NguoiNhans.Remove(donHang.NguoiNhan);

                    // Xóa SoNha của NguoiNhan nếu không còn người nhận khác
                    var conNguoiNhanCungSoNha = await _context.NguoiNhans.AnyAsync(nn => nn.MaSoNha == donHang.NguoiNhan.MaSoNha && nn.MaNguoiNhan != donHang.NguoiNhan.MaNguoiNhan);
                    if (!conNguoiNhanCungSoNha)
                    {
                        var soNhaNguoiNhan = await _context.SoNhas.FindAsync(donHang.NguoiNhan.MaSoNha);
                        if (soNhaNguoiNhan != null)
                            _context.SoNhas.Remove(soNhaNguoiNhan);
                    }
                }
            }

            // Xóa khách hàng nếu không còn đơn hàng khác tham chiếu
            if (donHang.KhachHang != null)
            {
                var conDonHangKhachHangKhac = await _context.DonHangs.AnyAsync(dh => dh.MaKhachHang == donHang.MaKhachHang && dh.MaDonHang != maDonHang);
                if (!conDonHangKhachHangKhac)
                {
                    // Xóa những người nhận của khách hàng này trước (vì NguoiNhan tham chiếu đến KhachHang)
                    var nguoiNhansCuaKhachHang = await _context.NguoiNhans.Where(nn => nn.MaKhachHang == donHang.MaKhachHang).ToListAsync();
                    _context.NguoiNhans.RemoveRange(nguoiNhansCuaKhachHang);

                    _context.KhachHangs.Remove(donHang.KhachHang);

                    // Xóa SoNha của KhachHang nếu không còn khách hàng khác
                    var conKhachHangCungSoNha = await _context.KhachHangs.AnyAsync(kh => kh.MaSoNha == donHang.KhachHang.MaSoNha && kh.MaKhachHang != donHang.MaKhachHang);
                    if (!conKhachHangCungSoNha)
                    {
                        var soNhaKhachHang = await _context.SoNhas.FindAsync(donHang.KhachHang.MaSoNha);
                        if (soNhaKhachHang != null)
                            _context.SoNhas.Remove(soNhaKhachHang);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Đã xóa đơn hàng và dữ liệu liên quan (nếu có).");
        }


        [HttpGet("DSduyet")]
        public async Task<IActionResult> GetDonHangsDuyet()
        {
            var donHangsRaw = await _context.DonHangs
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.SoNha)
                        .ThenInclude(sn => sn.PhuongXa)
                            .ThenInclude(px => px.QuanHuyen)
                                .ThenInclude(qh => qh.TinhThanhPho)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.NguoiNhans)
                        .ThenInclude(nn => nn.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
                .Where(dh => dh.TrangThaiDonHang == "chờ phân công")
                .ToListAsync();

            var donHangs = donHangsRaw.Select(dh =>
            {
                var nguoiNhan = dh.KhachHang.NguoiNhans.FirstOrDefault();

                string diaChiNguoiNhan = "Không xác định";
                if (nguoiNhan?.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiNguoiNhan = nguoiNhan.SoNha.DiaChiSoNha + ", " +
                                      nguoiNhan.SoNha.PhuongXa.TenPhuongXa + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                string diaChiKhachHang = "Không xác định";
                if (dh.KhachHang.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiKhachHang = dh.KhachHang.SoNha.DiaChiSoNha + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.TenPhuongXa + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                return new
                {
                    maDonHang = dh.MaDonHang,
                    maVanDon = dh.MaVanDon,
                    tenDonHang = dh.TenDonHang,
                    tenKhachHang = dh.KhachHang.TenKhachHang,
                    sdtKhachHang = dh.KhachHang.SDT,
                    diaChiKhachHang = diaChiKhachHang,
                    hoTenNguoiNhan = nguoiNhan?.HoTen ?? "Không xác định",
                    sdtNguoiNhan = nguoiNhan?.SDT ?? "Không xác định",
                    diaChiNguoiNhan = diaChiNguoiNhan,
                    phiGiaoHang = dh.PhiGiaoHang,
                    ngayGui = dh.NgayGui,
                    trangThaiDonHang = dh.TrangThaiDonHang
                };
            }).ToList();

            return Ok(donHangs);
        }


        [HttpGet("danggiaohang")]
        public async Task<IActionResult> GetDonHangsDangGiao()
        {
            var donHangsRaw = await _context.DonHangs
                .Include(dh => dh.NhanVien)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.SoNha)
                        .ThenInclude(sn => sn.PhuongXa)
                            .ThenInclude(px => px.QuanHuyen)
                                .ThenInclude(qh => qh.TinhThanhPho)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.NguoiNhans)
                        .ThenInclude(nn => nn.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
              .Where(dh => dh.TrangThaiDonHang == "Đã duyệt" || dh.TrangThaiDonHang == "trả về kho" || dh.TrangThaiDonHang == "đang giao" || dh.TrangThaiDonHang == "Khách hẹn lại ngày giao")
                .ToListAsync();

            var donHangs = donHangsRaw.Select(dh =>
            {
                var nguoiNhan = dh.KhachHang.NguoiNhans.FirstOrDefault();

                string diaChiNguoiNhan = "Không xác định";
                if (nguoiNhan?.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiNguoiNhan = nguoiNhan.SoNha.DiaChiSoNha + ", " +
                                      nguoiNhan.SoNha.PhuongXa.TenPhuongXa + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                string diaChiKhachHang = "Không xác định";
                if (dh.KhachHang.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiKhachHang = dh.KhachHang.SoNha.DiaChiSoNha + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.TenPhuongXa + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                return new
                {
                    maDonHang = dh.MaDonHang,
                    maVanDon = dh.MaVanDon,
                    tenNhanVien = dh.NhanVien?.TenNhanVien ?? "Chưa có nhân viên",
                    tenDonHang = dh.TenDonHang,
                    tenKhachHang = dh.KhachHang.TenKhachHang,
                    sdtKhachHang = dh.KhachHang.SDT,
                    diaChiKhachHang = diaChiKhachHang,
                    hoTenNguoiNhan = nguoiNhan?.HoTen ?? "Không xác định",
                    sdtNguoiNhan = nguoiNhan?.SDT ?? "Không xác định",
                    diaChiNguoiNhan = diaChiNguoiNhan,
                    phiGiaoHang = dh.PhiGiaoHang,
                    ngayGui = dh.NgayGui,
                    trangThaiDonHang = dh.TrangThaiDonHang
                };
            }).ToList();

            return Ok(donHangs);
        }


        // API để duyệt đơn hàng và tạo mã vận đơn
        [HttpPut("duyet/{maDonHang}")]
        public async Task<IActionResult> DuyetDonHang(string maDonHang)
        {
            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (donHang == null)
            {
                return NotFound("Đơn hàng không tồn tại.");
            }

            // Tạo mã vận đơn mới (ví dụ đơn giản là mã vận đơn sẽ là mã đơn hàng kèm theo thời gian)
            string maVanDon = "VD-" + maDonHang + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Cập nhật trạng thái đơn hàng và mã vận đơn
            donHang.TrangThaiDonHang = "chờ phân công";
            donHang.MaVanDon = maVanDon;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về thông tin đơn hàng đã duyệt
            return Ok(new DonHangDto
            {
                MaDonHang = donHang.MaDonHang,
                TenDonHang = donHang.TenDonHang,
                PhiGiaoHang = donHang.PhiGiaoHang,
                NgayGui = donHang.NgayGui,
                TrangThaiDonHang = donHang.TrangThaiDonHang,
                MaVanDon = donHang.MaVanDon
            });
        }




        [HttpPost("phancong")]
        public async Task<IActionResult> PhanCongDonHang([FromBody] PhanCongDto dto)
        {
            var nhanVien = await _context.NhanViens.FindAsync(dto.MaNhanVien);
            if (nhanVien == null) return BadRequest("Nhân viên không tồn tại.");

            var donHangs = await _context.DonHangs
                .Where(dh => dto.DanhSachDonHang.Contains(dh.MaDonHang))
                .ToListAsync();

            foreach (var dh in donHangs)
            {
                dh.MaNhanVien = dto.MaNhanVien;
                dh.TrangThaiDonHang = "Đã duyệt";
            }

            await _context.SaveChangesAsync();
            return Ok("Phân công thành công.");
        }



        /// <summary>
        /// ĐẶC BIỆT
        /// </summary>
        /// <returns></returns>

        [HttpGet("DSDonHangDacBiet")]
        public async Task<IActionResult> GetDonHangsDacBiet()
        {
            var excludedStatuses = new[] { "đã giao", "chờ duyệt", "chờ phân công", "đang giao", "Đã duyệt", "trả về kho" };

            var donHangsRaw = await _context.DonHangs
                  .Include(dh => dh.NhanVien)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.SoNha)
                        .ThenInclude(sn => sn.PhuongXa)
                            .ThenInclude(px => px.QuanHuyen)
                                .ThenInclude(qh => qh.TinhThanhPho)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.NguoiNhans)
                        .ThenInclude(nn => nn.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
                .Where(dh => !excludedStatuses.Contains(dh.TrangThaiDonHang.ToLower()))
                .ToListAsync();

            var donHangs = donHangsRaw.Select(dh =>
            {
                var nguoiNhan = dh.KhachHang.NguoiNhans.FirstOrDefault();

                string diaChiNguoiNhan = "Không xác định";
                if (nguoiNhan?.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiNguoiNhan = nguoiNhan.SoNha.DiaChiSoNha + ", " +
                                      nguoiNhan.SoNha.PhuongXa.TenPhuongXa + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                      nguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                string diaChiKhachHang = "Không xác định";
                if (dh.KhachHang.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                {
                    diaChiKhachHang = dh.KhachHang.SoNha.DiaChiSoNha + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.TenPhuongXa + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                     dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                }

                return new
                {
                    maDonHang = dh.MaDonHang,
                    maVanDon = dh.MaVanDon,
                    tenNhanVien = dh.NhanVien?.TenNhanVien ?? "Chưa có nhân viên",
                    sdtNhanvien = dh.NhanVien?.SDT ?? "Chưa có SDT",
                    tenDonHang = dh.TenDonHang,
                    tenKhachHang = dh.KhachHang.TenKhachHang,
                    sdtKhachHang = dh.KhachHang.SDT,
                    diaChiKhachHang = diaChiKhachHang,
                    hoTenNguoiNhan = nguoiNhan?.HoTen ?? "Không xác định",
                    sdtNguoiNhan = nguoiNhan?.SDT ?? "Không xác định",
                    diaChiNguoiNhan = diaChiNguoiNhan,
                    phiGiaoHang = dh.PhiGiaoHang,
                    ngayGui = dh.NgayGui,
                    trangThaiDonHang = dh.TrangThaiDonHang,
                    lydo = dh.GhiChu
                };
            }).ToList();

            return Ok(donHangs);
        }
        [HttpPut("CapNhatTrangThaiTuDong")]
        public async Task<IActionResult> CapNhatTrangThaiTuDong(string maDonHang)
        {
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            if (donHang == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            string trangThaiHienTai = donHang.TrangThaiDonHang.ToLower();

            if (trangThaiHienTai == "khách không nhận hàng")
            {
                donHang.TrangThaiDonHang = "trả về kho";
            }
            else if (trangThaiHienTai == "không tiếp nhận")
            {
                donHang.TrangThaiDonHang = "chờ phân công";
            }
            else
            {
                return BadRequest("Đơn hàng không ở trạng thái có thể cập nhật tự động.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Cập nhật trạng thái đơn hàng {maDonHang} thành '{donHang.TrangThaiDonHang}' thành công." });
        }
        [HttpPut("HuyYeuCau")]
        public async Task<IActionResult> HuyYeuCau(string maDonHang)
        {
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            if (donHang == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            string trangThai = donHang.TrangThaiDonHang.ToLower();

            if (trangThai == "không tiếp nhận")
            {
                donHang.TrangThaiDonHang = "Đã duyệt";
            }
            else if (trangThai == "khách không nhận hàng")
            {
                donHang.TrangThaiDonHang = "Đã duyệt";
            }
            else
            {
                return BadRequest("Chỉ có thể hủy yêu cầu khi trạng thái là 'không tiếp nhận' hoặc 'hoàn hàng'.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đơn hàng {maDonHang} đã được cập nhật trạng thái về 'Đã duyệt'." });
        }







        [HttpGet("GetNhanVienPhuHopDiaChi/{maDonHang}")]
        public IActionResult GetNhanVienPhuHopDiaChi(string maDonHang)
        {
            // Lấy đơn hàng, bao gồm KhachHang, NguoiNhan và địa chỉ người nhận (không lấy số nhà)
            var donHang = _context.DonHangs
                .Include(d => d.KhachHang)
                    .ThenInclude(kh => kh.NguoiNhans)
                        .ThenInclude(nn => nn.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
                .FirstOrDefault(d => d.MaDonHang == maDonHang);

            if (donHang == null)
                return NotFound("Không tìm thấy đơn hàng.");

            var nguoiNhan = donHang.KhachHang?.NguoiNhans?.FirstOrDefault();

            if (nguoiNhan == null || nguoiNhan.SoNha == null || nguoiNhan.SoNha.PhuongXa == null)
                return BadRequest("Không thể xác định địa chỉ người nhận.");

            var px = nguoiNhan.SoNha.PhuongXa;
            var qh = px.QuanHuyen;
            var tp = qh?.TinhThanhPho;

            if (qh == null || tp == null)
                return BadRequest("Địa chỉ người nhận không đầy đủ.");

            // Mã tỉnh, quận, phường của người nhận
            var maPhuongXa = px.MaPhuongXa;
            var maQuanHuyen = qh.MaQuanHuyen;
            var maTinhTP = tp.MaTinhTP;

            // Lấy danh sách nhân viên giao hàng có địa chỉ phường, quận, tỉnh trùng với người nhận
            var nhanViensPhuHop = _context.NhanViens
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                        .ThenInclude(p => p.QuanHuyen)
                            .ThenInclude(q => q.TinhThanhPho)
                .Where(nv =>
                        nv.VaiTro.TenVaiTro == "Nhân viên giao hàng" &&
                    nv.SoNha.PhuongXa.MaPhuongXa == maPhuongXa &&
                    nv.SoNha.PhuongXa.QuanHuyen.MaQuanHuyen == maQuanHuyen &&
                    nv.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP == maTinhTP
                )
                .Select(nv => new
                {
                    nv.MaNhanVien,
                    nv.TenNhanVien,
                    nv.SDT,  // số điện thoại nhân viên
                    DiaChi = nv.SoNha.DiaChiSoNha + ", " +
                 nv.SoNha.PhuongXa.TenPhuongXa + ", " +
                 nv.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                 nv.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP
                })
                .ToList();

            return Ok(nhanViensPhuHop);
        }


        [HttpGet("LocDonHangChoPhanCong")]
        public async Task<IActionResult> LocDonHangChoPhanCong(
       [FromQuery] string? maTinhTP,
       [FromQuery] string? maQuanHuyen,
       [FromQuery] string? maPhuongXa)
        {
            var donHangsRaw = await _context.DonHangs
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.SoNha)
                        .ThenInclude(sn => sn.PhuongXa)
                            .ThenInclude(px => px.QuanHuyen)
                                .ThenInclude(qh => qh.TinhThanhPho)
                .Include(dh => dh.KhachHang)
                    .ThenInclude(kh => kh.NguoiNhans)
                        .ThenInclude(nn => nn.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
                .Where(dh => dh.TrangThaiDonHang == "chờ phân công")
                .ToListAsync();

            var donHangs = donHangsRaw
                .Where(dh =>
                {
                    var nguoiNhan = dh.KhachHang?.NguoiNhans?.FirstOrDefault();
                    var phuongXa = nguoiNhan?.SoNha?.PhuongXa;
                    var quanHuyen = phuongXa?.QuanHuyen;
                    var tinh = quanHuyen?.TinhThanhPho;

                    bool matchTinh = string.IsNullOrEmpty(maTinhTP) || tinh?.MaTinhTP == maTinhTP;
                    bool matchQuan = string.IsNullOrEmpty(maQuanHuyen) || quanHuyen?.MaQuanHuyen == maQuanHuyen;
                    bool matchPhuong = string.IsNullOrEmpty(maPhuongXa) || phuongXa?.MaPhuongXa == maPhuongXa;

                    return matchTinh && matchQuan && matchPhuong;
                })
                .Select(dh =>
                {
                    var nguoiNhan = dh.KhachHang?.NguoiNhans?.FirstOrDefault();

                    string diaChiNguoiNhan = "Không xác định";
                    if (nguoiNhan?.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                    {
                        diaChiNguoiNhan = nguoiNhan.SoNha.DiaChiSoNha + ", " +
                                          nguoiNhan.SoNha.PhuongXa.TenPhuongXa + ", " +
                                          nguoiNhan.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                          nguoiNhan.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                    }

                    string diaChiKhachHang = "Không xác định";
                    if (dh.KhachHang?.SoNha?.PhuongXa?.QuanHuyen?.TinhThanhPho != null)
                    {
                        diaChiKhachHang = dh.KhachHang.SoNha.DiaChiSoNha + ", " +
                                          dh.KhachHang.SoNha.PhuongXa.TenPhuongXa + ", " +
                                          dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                                          dh.KhachHang.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP;
                    }

                    return new
                    {
                        maDonHang = dh.MaDonHang,
                        maVanDon = dh.MaVanDon,
                        tenDonHang = dh.TenDonHang,
                        tenKhachHang = dh.KhachHang?.TenKhachHang ?? "Không xác định",
                        sdtKhachHang = dh.KhachHang?.SDT ?? "Không xác định",
                        diaChiKhachHang = diaChiKhachHang,
                        hoTenNguoiNhan = nguoiNhan?.HoTen ?? "Không xác định",
                        sdtNguoiNhan = nguoiNhan?.SDT ?? "Không xác định",
                        diaChiNguoiNhan = diaChiNguoiNhan,
                        phiGiaoHang = dh.PhiGiaoHang,
                        ngayGui = dh.NgayGui,
                        trangThaiDonHang = dh.TrangThaiDonHang
                    };
                })
                .ToList();

            return Ok(donHangs);
        }



        [HttpGet("ThongKeDonHang")]
        public IActionResult ThongKeDonHang(int tuThang, int denThang, int nam)
        {
            var thongKe = new List<object>();

            for (int thang = tuThang; thang <= denThang; thang++)
            {
                var soDonGiao = _context.DonHangs
                    .Where(d => d.NgayNhan.HasValue &&
                                d.NgayNhan.Value.Month == thang &&
                                d.NgayNhan.Value.Year == nam &&
                                d.TrangThaiDonHang == "đã giao")
                    .Count();

                var soDonHoan = _context.DonHangs
                    .Where(d => d.NgayNhan.HasValue &&
                                d.NgayNhan.Value.Month == thang &&
                                d.NgayNhan.Value.Year == nam &&
                                d.TrangThaiDonHang == "hoàn hàng")
                    .Count();

                thongKe.Add(new
                {
                    thang = thang,
                    soDonGiao = soDonGiao,
                    soDonHoan = soDonHoan
                });
            }

            return Ok(thongKe);
        }

        [HttpGet("GetAllKhachHangs")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllKhachHangs()
        {
            var khachHangs = await _context.KhachHangs
                .Include(k => k.SoNha)
                    .ThenInclude(s => s.PhuongXa)
                    .ThenInclude(p => p.QuanHuyen)
                    .ThenInclude(q => q.TinhThanhPho)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.TenKhachHang,
                    k.SDT,
                    k.Email,
                    k.NgaySinh,
                    k.CCCD,
                    k.MaSoNha,
                    MaTinhTP = k.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP,
                    TenTinhTP = k.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP,
                    MaQuanHuyen = k.SoNha.PhuongXa.QuanHuyen.MaQuanHuyen,
                    TenQuanHuyen = k.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen,
                    MaPhuongXa = k.SoNha.PhuongXa.MaPhuongXa,
                    TenPhuongXa = k.SoNha.PhuongXa.TenPhuongXa,
                    DiaChiSoNha = k.SoNha.DiaChiSoNha,
                    DiaChiChiTiet = $"{k.SoNha.DiaChiSoNha}, {k.SoNha.PhuongXa.TenPhuongXa}, {k.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen}, {k.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP}"
                })
                .ToListAsync();

            return Ok(khachHangs);
        }

        [HttpPost("tao")]
        public async Task<IActionResult> TaoDonHang([FromBody] TaoDonHangDto dto)
        {
            try
            {
                // === LẤY TẤT CẢ MÃ HIỆN CÓ ===
                var allMaSoNha = await _context.SoNhas
                    .Where(sn => sn.MaSoNha.StartsWith("SN"))
                    .Select(sn => sn.MaSoNha)
                    .ToListAsync();

                var allMaDonHang = await _context.DonHangs
                    .Where(d => d.MaDonHang.StartsWith("DH"))
                    .Select(d => d.MaDonHang)
                    .ToListAsync();



                var allMaKhachHang = await _context.KhachHangs
                    .Where(kh => kh.MaKhachHang.StartsWith("KH"))
                    .Select(kh => kh.MaKhachHang)
                    .ToListAsync();

                var allMaNguoiNhan = await _context.NguoiNhans
                    .Where(nn => nn.MaNguoiNhan.StartsWith("NN"))
                    .Select(nn => nn.MaNguoiNhan)
                    .ToListAsync();

                var allMaChiTietDonHang = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaChiTietDonHang.StartsWith("CT"))
                    .Select(ct => ct.MaChiTietDonHang)
                    .ToListAsync();

                // === TẠO CÁC MÃ MỚI ===
                var maSoNhaKH = GenerateNextCode(allMaSoNha, "SN");
                allMaSoNha.Add(maSoNhaKH);

                var maSoNhaNN = GenerateNextCode(allMaSoNha, "SN");

                var newMaDonHang = GenerateNextCode(allMaDonHang, "DH");
                var newMaVanDon = $"VD-{newMaDonHang}-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                var newMaKhachHang = GenerateNextCode(allMaKhachHang, "KH");
                var newMaNguoiNhan = GenerateNextCode(allMaNguoiNhan, "NN");

                // === TẠO ĐỊA CHỈ KHÁCH HÀNG ===
                var soNhaKH = new SoNha
                {
                    MaSoNha = maSoNhaKH,
                    DiaChiSoNha = dto.DiaChiKhachHang.SoNha,
                    MaPhuongXa = dto.DiaChiKhachHang.MaPhuongXa
                };
                _context.SoNhas.Add(soNhaKH);

                // === TẠO ĐỊA CHỈ NGƯỜI NHẬN ===
                var soNhaNN = new SoNha
                {
                    MaSoNha = maSoNhaNN,
                    DiaChiSoNha = dto.DiaChiNguoiNhan.SoNha,
                    MaPhuongXa = dto.DiaChiNguoiNhan.MaPhuongXa
                };
                _context.SoNhas.Add(soNhaNN);

                await _context.SaveChangesAsync();

                // === TẠO KHÁCH HÀNG ===
                var khachHang = new KhachHang
                {
                    MaKhachHang = newMaKhachHang,
                    TenKhachHang = dto.TenKhachHang,
                    SDT = dto.SDT_KhachHang,
                    MaSoNha = soNhaKH.MaSoNha
                };

                // === TẠO NGƯỜI NHẬN ===
                var nguoiNhan = new NguoiNhan
                {
                    MaNguoiNhan = newMaNguoiNhan,
                    HoTen = dto.HoTenNguoiNhan,
                    SDT = dto.SDT_NguoiNhan,
                    MaSoNha = soNhaNN.MaSoNha,
                    MaKhachHang = khachHang.MaKhachHang
                };

                // === TẠO ĐƠN HÀNG ===
                var donHang = new DonHang
                {
                    MaDonHang = newMaDonHang,
                    MaVanDon = newMaVanDon,
                    TenDonHang = dto.TenDonHang,
                    TienThuHo = dto.TienThuHo,
                    PhiGiaoHang = dto.PhiGiaoHang,
                    NgayGui = dto.NgayGui,
                    TrangThaiDonHang = "chờ phân công",
                    MaNguoiNhan = nguoiNhan.MaNguoiNhan,
                    NguoiTraPhi = dto.NguoiTraPhi,
                    KhachHang = khachHang
                };
                // === XỬ LÝ THÔNG TIN THANH TOÁN THEO NGƯỜI TRẢ PHÍ ===
                if (dto.NguoiTraPhi?.ToLower() == "người gửi")
                {
                    donHang.TrangThaiThanhToan = "Đã thanh toán";
                    donHang.PhuongThucThanhToan = "Tiền mặt";
                    donHang.TrangThaiThuHo = "Đã thu";
                }

                // === TẠO CHI TIẾT ĐƠN HÀNG ===
                foreach (var hangDto in dto.HangHoas)
                {
                    var hangHoa = await _context.HangHoas.FindAsync(hangDto.MaHangHoa);
                    if (hangHoa == null)
                        return BadRequest($"Hàng hóa với mã {hangDto.MaHangHoa} không tồn tại.");

                    var danhMuc = await _context.DanhMucs.FindAsync(hangDto.MaDanhMuc);
                    if (danhMuc == null)
                        return BadRequest($"Danh mục với mã {hangDto.MaDanhMuc} không tồn tại.");

                    if (hangHoa.MaDanhMuc != hangDto.MaDanhMuc)
                        return BadRequest($"Mã danh mục '{hangDto.MaDanhMuc}' không khớp với hàng hóa '{hangHoa.MaDanhMuc}'.");

                    // Tạo mã chi tiết đơn hàng theo định dạng CT01, CT02...
                    var newMaChiTiet = GenerateNextCode(allMaChiTietDonHang, "CT");
                    allMaChiTietDonHang.Add(newMaChiTiet); // đảm bảo không trùng khi lặp tiếp

                    var chiTiet = new ChiTietDonHang
                    {
                        MaChiTietDonHang = newMaChiTiet,
                        DonHang = donHang,
                        HangHoa = hangHoa,
                        SoLuong = hangDto.SoLuong,
                        TrongLuong = hangDto.TrongLuong,
                        KichThuoc = hangDto.KichThuoc,
                        MaHangHoa = hangDto.MaHangHoa
                    };

                    _context.ChiTietDonHangs.Add(chiTiet);
                }

                _context.KhachHangs.Add(khachHang);
                _context.NguoiNhans.Add(nguoiNhan);
                _context.DonHangs.Add(donHang);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Tạo đơn hàng thành công!",
                    donHang.MaDonHang,
                    donHang.MaVanDon
                });
            }
            catch (Exception ex)
            {
                string errorDetail = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine("Lỗi khi tạo đơn hàng:");
                Console.WriteLine(errorDetail);
                return StatusCode(500, $"Lỗi server: {errorDetail}");
            }
        }

        private string GenerateNextCode(IEnumerable<string> existingCodes, string prefix)
        {
            int i = 1;
            while (true)
            {
                string code = $"{prefix}{i:D2}";
                if (!existingCodes.Contains(code))
                    return code;
                i++;
            }
        }

        [HttpPut("sua/{maDonHang}")]
        public async Task<IActionResult> SuaDonHang(string maDonHang, [FromBody] SuaDonHangDto dto)
        {
            try
            {
                // Tìm đơn hàng cần sửa, bao gồm dữ liệu liên quan
                var donHang = await _context.DonHangs
                    .Include(d => d.KhachHang)
                    .ThenInclude(kh => kh.SoNha)
                    .Include(d => d.KhachHang.NguoiNhans)
                    .Include(d => d.ChiTietDonHangs)
                    .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

                if (donHang == null)
                    return NotFound($"Không tìm thấy đơn hàng với mã {maDonHang}");

                // --- Cập nhật địa chỉ Khách hàng ---
                var soNhaKH = await _context.SoNhas.FindAsync(donHang.KhachHang.MaSoNha);
                if (soNhaKH != null)
                {
                    soNhaKH.DiaChiSoNha = dto.DiaChiKhachHang.SoNha;
                    soNhaKH.MaPhuongXa = dto.DiaChiKhachHang.MaPhuongXa;
                }

                // --- Cập nhật địa chỉ Người nhận ---
                // Giả sử chỉ có 1 người nhận chính (thay thế hoặc cập nhật)
                var nguoiNhan = donHang.KhachHang.NguoiNhans.FirstOrDefault();
                if (nguoiNhan != null)
                {
                    var soNhaNN = await _context.SoNhas.FindAsync(nguoiNhan.MaSoNha);
                    if (soNhaNN != null)
                    {
                        soNhaNN.DiaChiSoNha = dto.DiaChiNguoiNhan.SoNha;
                        soNhaNN.MaPhuongXa = dto.DiaChiNguoiNhan.MaPhuongXa;
                    }

                    nguoiNhan.HoTen = dto.HoTenNguoiNhan;
                    nguoiNhan.SDT = dto.SDT_NguoiNhan;
                }
                else
                {
                    // Nếu không có người nhận, có thể tạo mới
                    var newMaNguoiNhan = Guid.NewGuid().ToString();
                    var soNhaNN = new SoNha
                    {
                        MaSoNha = GenerateNextCode(await _context.SoNhas.Select(sn => sn.MaSoNha).ToListAsync(), "SN"),
                        DiaChiSoNha = dto.DiaChiNguoiNhan.SoNha,
                        MaPhuongXa = dto.DiaChiNguoiNhan.MaPhuongXa
                    };
                    _context.SoNhas.Add(soNhaNN);

                    var newNguoiNhan = new NguoiNhan
                    {
                        MaNguoiNhan = newMaNguoiNhan,
                        HoTen = dto.HoTenNguoiNhan,
                        SDT = dto.SDT_NguoiNhan,
                        MaSoNha = soNhaNN.MaSoNha,
                        MaKhachHang = donHang.KhachHang.MaKhachHang
                    };
                    _context.NguoiNhans.Add(newNguoiNhan);
                }

                // --- Cập nhật Khách hàng ---
                donHang.KhachHang.TenKhachHang = dto.TenKhachHang;
                donHang.KhachHang.SDT = dto.SDT_KhachHang;

                // --- Cập nhật đơn hàng ---
                donHang.TenDonHang = dto.TenDonHang;
                donHang.TienThuHo = dto.TienThuHo;
                donHang.PhiGiaoHang = dto.PhiGiaoHang;
                donHang.NgayGui = dto.NgayGui;
                donHang.NguoiTraPhi = dto.NguoiTraPhi;
                // --- Xử lý trạng thái thanh toán theo người trả phí ---
                if (dto.NguoiTraPhi?.ToLower() == "người gửi")
                {
                    donHang.TrangThaiThanhToan = "Đã thanh toán";
                    donHang.PhuongThucThanhToan = "Tiền mặt";
                    donHang.TrangThaiThuHo = "Đã thu";
                }

                // --- Xóa chi tiết đơn hàng cũ ---
                _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);

                // --- Thêm chi tiết đơn hàng mới ---
                foreach (var hangDto in dto.HangHoas)
                {
                    var hangHoa = await _context.HangHoas.FindAsync(hangDto.MaHangHoa);
                    if (hangHoa == null)
                        return BadRequest($"Hàng hóa với mã {hangDto.MaHangHoa} không tồn tại.");

                    var danhMuc = await _context.DanhMucs.FindAsync(hangDto.MaDanhMuc);
                    if (danhMuc == null)
                        return BadRequest($"Danh mục với mã {hangDto.MaDanhMuc} không tồn tại.");

                    if (hangHoa.MaDanhMuc != hangDto.MaDanhMuc)
                        return BadRequest($"Mã danh mục '{hangDto.MaDanhMuc}' không khớp với hàng hóa '{hangHoa.MaDanhMuc}'.");

                    var chiTiet = new ChiTietDonHang
                    {
                        MaChiTietDonHang = Guid.NewGuid().ToString(),
                        DonHang = donHang,
                        HangHoa = hangHoa,
                        SoLuong = hangDto.SoLuong,
                        TrongLuong = hangDto.TrongLuong,
                        KichThuoc = hangDto.KichThuoc,
                        MaHangHoa = hangDto.MaHangHoa
                    };

                    _context.ChiTietDonHangs.Add(chiTiet);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Sửa đơn hàng thành công!",
                    donHang.MaDonHang
                });
            }
            catch (Exception ex)
            {
                string errorDetail = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine("Lỗi khi sửa đơn hàng:");
                Console.WriteLine(errorDetail);
                return StatusCode(500, $"Lỗi server: {errorDetail}");
            }
        }

        [HttpGet("{maDonHang}")]
        public async Task<IActionResult> GetDonHangTheoMa(string maDonHang)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(dh => dh.KhachHang)
                        .ThenInclude(kh => kh.SoNha)
                            .ThenInclude(sn => sn.PhuongXa)
                                .ThenInclude(px => px.QuanHuyen)
                                    .ThenInclude(qh => qh.TinhThanhPho)
                    .Include(dh => dh.KhachHang)
                        .ThenInclude(kh => kh.NguoiNhans)
                            .ThenInclude(nn => nn.SoNha)
                                .ThenInclude(sn => sn.PhuongXa)
                                    .ThenInclude(px => px.QuanHuyen)
                                        .ThenInclude(qh => qh.TinhThanhPho)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.HangHoa)
                            .ThenInclude(hh => hh.DanhMuc)

                    .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

                if (donHang == null)
                    return NotFound("Không tìm thấy đơn hàng.");

                var nguoiNhan = donHang.KhachHang.NguoiNhans.FirstOrDefault();
                var soNhaNN = nguoiNhan?.SoNha;
                var soNhaKH = donHang.KhachHang.SoNha;

                var result = new
                {
                    donHang.MaDonHang,
                    donHang.MaVanDon,
                    donHang.TenDonHang,
                    donHang.TienThuHo,
                    donHang.PhiGiaoHang,
                    donHang.NgayGui,
                    donHang.TrangThaiDonHang,
                    NguoiTraPhi = donHang.NguoiTraPhi,


                    KhachHang = new
                    {
                        donHang.KhachHang.TenKhachHang,
                        donHang.KhachHang.SDT,
                        DiaChi = soNhaKH != null ? new
                        {
                            soNhaKH.DiaChiSoNha,
                            MaPhuongXa = soNhaKH.PhuongXa.MaPhuongXa,
                            TenPhuongXa = soNhaKH.PhuongXa.TenPhuongXa,
                            MaQuanHuyen = soNhaKH.PhuongXa.QuanHuyen.MaQuanHuyen,
                            TenQuanHuyen = soNhaKH.PhuongXa.QuanHuyen.TenQuanHuyen,
                            MaTinhTP = soNhaKH.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP,
                            TenTinhTP = soNhaKH.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP
                        } : null
                    },

                    NguoiNhan = nguoiNhan != null ? new
                    {
                        nguoiNhan.HoTen,
                        nguoiNhan.SDT,
                        DiaChi = soNhaNN != null ? new
                        {
                            soNhaNN.DiaChiSoNha,
                            MaPhuongXa = soNhaNN.PhuongXa.MaPhuongXa,
                            TenPhuongXa = soNhaNN.PhuongXa.TenPhuongXa,
                            MaQuanHuyen = soNhaNN.PhuongXa.QuanHuyen.MaQuanHuyen,
                            TenQuanHuyen = soNhaNN.PhuongXa.QuanHuyen.TenQuanHuyen,
                            MaTinhTP = soNhaNN.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP,
                            TenTinhTP = soNhaNN.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP
                        } : null
                    } : null,

                    HangHoas = donHang.ChiTietDonHangs.Select(ct => new
                    {
                        ct.MaHangHoa,
                        ct.HangHoa.TinhChatHangHoa,
                        ct.SoLuong,
                        ct.TrongLuong,
                        ct.KichThuoc,
                        MaDanhMuc = ct.HangHoa.DanhMuc?.MaDanhMuc,
                        TenDanhMuc = ct.HangHoa.DanhMuc?.TenDanhMuc,
                        DonGia = ct.HangHoa.DonGia
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }



    }
}