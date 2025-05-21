using Microsoft.AspNetCore.Mvc;
using Nhom3_CongTyVanChuyen.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Dtos;

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
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (donHang == null)
                return NotFound("Đơn hàng không tồn tại.");

            _context.DonHangs.Remove(donHang);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa đơn hàng.");
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
              .Where(dh => dh.TrangThaiDonHang == "Đã duyệt" || dh.TrangThaiDonHang == "trả về kho" || dh.TrangThaiDonHang == "Khách hẹn lại ngày giao")
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

            string trangThaiHienTai = donHang.TrangThaiDonHang?.ToLower();

            if (trangThaiHienTai == "hoàn hàng")
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

            string trangThai = donHang.TrangThaiDonHang?.ToLower();

            if (trangThai == "không tiếp nhận")
            {
                donHang.TrangThaiDonHang = "Đã duyệt";
            }
            else if (trangThai == "hoàn hàng")
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
                        nv.VaiTro.TenVaiTro== "Nhân viên giao hàng" &&
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
    }



}





