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
                .Where(dh => dh.TrangThaiDonHang == "đang giao")
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
        [HttpGet("nhanvien-giaohang")]


        public async Task<IActionResult> GetNhanVienGiaoHang()
        {
            var nhanViens = await _context.NhanViens
                .Where(nv => nv.VaiTro.TenVaiTro == "Nhân viên giao hàng")
                .Select(nv => new
                {
                    nv.MaNhanVien,
                    nv.TenNhanVien,
                    TenQuanHuyen = nv.SoNha.PhuongXa.TenPhuongXa
                })
                .ToListAsync();

            return Ok(nhanViens);
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



        [HttpPost("tao")]

        public async Task<IActionResult> TaoDonHang([FromBody] TaoDonHangDto dto)
        {
            try
            {
                // ======= 1. Tạo địa chỉ khách hàng =======
                var maSoNhaKH = "SN" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                var diaChiDayDuKH = await GetDiaChiDayDu(dto.DiaChiKhachHang);
                var soNhaKH = new SoNha
                {
                    MaSoNha = maSoNhaKH,
                    DiaChiSoNha = $"{dto.DiaChiKhachHang.SoNha}, {diaChiDayDuKH}",
                    MaPhuongXa = dto.DiaChiKhachHang.MaPhuongXa
                };
                _context.SoNhas.Add(soNhaKH);

                // =====`== 2. Tạo địa chỉ người nhận =======
                var maSoNhaNN = "SN" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                var diaChiDayDuNN = await GetDiaChiDayDu(dto.DiaChiNguoiNhan);
                var soNhaNN = new SoNha
                {
                    MaSoNha = maSoNhaNN,
                    DiaChiSoNha = $"{dto.DiaChiNguoiNhan.SoNha}, {diaChiDayDuNN}",
                    MaPhuongXa = dto.DiaChiNguoiNhan.MaPhuongXa
                };
                _context.SoNhas.Add(soNhaNN);
                await _context.SaveChangesAsync();

                // ======= 3. Tạo khách hàng =======
                var khachHang = new KhachHang
                {
                    MaKhachHang = Guid.NewGuid().ToString(),
                    TenKhachHang = dto.TenKhachHang,
                    SDT = dto.SDT_KhachHang,
                    MaSoNha = soNhaKH.MaSoNha
                };

                // ======= 4. Tạo người nhận =======
                var nguoiNhan = new NguoiNhan
                {
                    MaNguoiNhan = Guid.NewGuid().ToString(),
                    HoTen = dto.HoTenNguoiNhan,
                    SDT = dto.SDT_NguoiNhan,
                    MaSoNha = soNhaNN.MaSoNha
                };

                // ======= 5. Tạo mã đơn hàng tự động: DH001, DH002... =======
                string newMaDonHang;
                var lastOrder = await _context.DonHangs
                    .OrderByDescending(d => d.MaDonHang)
                    .FirstOrDefaultAsync();

                if (lastOrder != null && lastOrder.MaDonHang.StartsWith("DH"))
                {
                    var numberPart = lastOrder.MaDonHang.Substring(2);
                    int.TryParse(numberPart, out int lastNumber);
                    newMaDonHang = $"DH{(lastNumber + 1):D3}";
                }
                else
                {
                    newMaDonHang = "DH001";
                }

                // ======= 6. Tạo mã vận đơn tự động: VD1, VD2... =======
                string newMaVanDon;
                var lastVD = await _context.DonHangs
                    .OrderByDescending(d => d.MaVanDon)
                    .FirstOrDefaultAsync();

                if (lastVD != null && lastVD.MaVanDon != null && lastVD.MaVanDon.StartsWith("VD"))
                {
                    var vdNumber = lastVD.MaVanDon.Substring(2);
                    int.TryParse(vdNumber, out int lastVDNumber);
                    newMaVanDon = $"VD{(lastVDNumber + 1)}";
                }
                else
                {
                    newMaVanDon = "VD1";
                }

                // ======= 7. Tạo đơn hàng =======
                var donHang = new DonHang
                {
                    MaDonHang = newMaDonHang,
                    MaVanDon = newMaVanDon,
                    TenDonHang = dto.TenDonHang,
                    TienThuHo = dto.TienThuHo,
                    NgayGui = dto.NgayGui,
                    TrangThaiDonHang = "chờ duyệt",
                    KhachHang = khachHang,
                    //    NguoiNhans = new List<NguoiNhan> { nguoiNhan },
                    PhiGiaoHang = 0 // hoặc tính nếu có
                };

                // ======= 8. Hàng hóa và chi tiết đơn hàng =======
                foreach (var hangDto in dto.HangHoas)
                {
                    var danhMuc = await _context.DanhMucs.FindAsync(hangDto.MaDanhMuc);
                    if (danhMuc == null)
                        return BadRequest($"Danh mục với ID {hangDto.MaDanhMuc} không tồn tại.");

                    var hangHoa = new HangHoa
                    {
                        MaHangHoa = Guid.NewGuid().ToString(),
                        DonGia = hangDto.DonGia,
                        TinhChatHangHoa = hangDto.TinhChatHangHoa,
                        MaDanhMuc = hangDto.MaDanhMuc // Vẫn lưu bằng mã danh mục
                    };

                    var chiTiet = new ChiTietDonHang
                    {
                        MaChiTietDonHang = Guid.NewGuid().ToString(),
                        DonHang = donHang,
                        HangHoa = hangHoa,
                        SoLuong = hangDto.SoLuong,
                        TrongLuong = hangDto.TrongLuong,
                        KichThuoc = hangDto.KichThuoc
                    };

                    _context.ChiTietDonHangs.Add(chiTiet);
                }

                // ======= 9. Lưu DB =======
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
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        private async Task<string> GetDiaChiDayDu(DiaChiDto diaChi)
        {
            var xa = await _context.PhuongXas.FindAsync(diaChi.MaPhuongXa);
            var quan = await _context.QuanHuyens.FindAsync(diaChi.MaQuanHuyen);
            var tinh = await _context.TinhThanhPhos.FindAsync(diaChi.MaTinhThanhPho);

            return $"{xa?.TenPhuongXa ?? "Chưa rõ"}, {quan?.TenQuanHuyen ?? "Chưa rõ"}, {tinh?.TenTinhTP ?? "Chưa rõ"}";
        }



        [HttpGet("DSDonHangDacBiet")]
        public async Task<IActionResult> GetDonHangsDacBiet()
        {
            var excludedStatuses = new[] { "đã giao", "chờ duyệt", "chờ phân công", "đang giao" };

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



        [HttpGet("GetNhanVienPhuHop/{maDonHang}")]
        public IActionResult GetNhanVienPhuHop(string maDonHang)
        {
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

            if (nguoiNhan?.SoNha == null || nguoiNhan.SoNha.PhuongXa == null)
                return BadRequest("Không thể xác định địa chỉ người nhận.");

            // Lấy mã địa chỉ người nhận
            var maPhuongXa = nguoiNhan.SoNha.PhuongXa.MaPhuongXa;
            var maQuanHuyen = nguoiNhan.SoNha.PhuongXa.QuanHuyen?.MaQuanHuyen;
            var maTinhTP = nguoiNhan.SoNha.PhuongXa.QuanHuyen?.TinhThanhPho?.MaTinhTP;

            if (maQuanHuyen == null || maTinhTP == null)
                return BadRequest("Địa chỉ người nhận không đầy đủ.");

            // So sánh trực tiếp các mã
            var nhanViensPhuHop = _context.NhanViens
                .Include(nv => nv.SoNha)
                    .ThenInclude(sn => sn.PhuongXa)
                        .ThenInclude(px => px.QuanHuyen)
                            .ThenInclude(qh => qh.TinhThanhPho)
                .Where(nv =>
                    nv.VaiTro.MaVaiTro == "giao hàng" &&
                    nv.SoNha.PhuongXa.MaPhuongXa == maPhuongXa &&
                    nv.SoNha.PhuongXa.QuanHuyen.MaQuanHuyen == maQuanHuyen &&
                    nv.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.MaTinhTP == maTinhTP
                )
                .Select(nv => new
                {
                    nv.MaNhanVien,
                    nv.TenNhanVien,
                    DiaChi = nv.SoNha.DiaChiSoNha + ", " +
                             nv.SoNha.PhuongXa.TenPhuongXa + ", " +
                             nv.SoNha.PhuongXa.QuanHuyen.TenQuanHuyen + ", " +
                             nv.SoNha.PhuongXa.QuanHuyen.TinhThanhPho.TenTinhTP
                })
                .ToList();

            return Ok(nhanViensPhuHop);
        }

    }




}
