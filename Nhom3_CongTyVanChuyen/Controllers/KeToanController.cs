using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;
using Nhom3_CongTyVanChuyen.Dtos;
using System.Linq;
using System.Threading.Tasks;

namespace Nhom3_CongTyVanChuyen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KeToanController : ControllerBase
    {
        private readonly MyDbContext _context;

        public KeToanController(MyDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách đơn hàng đã giao
        [HttpGet("DeliveredOrders")]
        public async Task<IActionResult> GetDeliveredOrders()
        {
            var orders = await _context.DonHangs
                .Include(dh => dh.KhachHang)
                .Include(dh => dh.NhanVien)
                .Where(dh => dh.TrangThaiDonHang == "Đã giao")
                .Select(dh => new DeliveredOrderDto
                {
                    MaDonHang = dh.MaDonHang,
                    TenDonHang = dh.TenDonHang,
                    TenKhachHang = dh.KhachHang.TenKhachHang,
                    TenNhanVien = dh.NhanVien.TenNhanVien,
                    TienThuHo = dh.TienThuHo,
                    NgayNhan = dh.NgayNhan,
                    TrangThaiThuHo = dh.TrangThaiThuHo ?? "Chưa thu"
                })
                .ToListAsync();

            return Ok(orders);
        }

        // Cập nhật trạng thái thu hộ
        [HttpPost("UpdateCODStatus")]
        public async Task<IActionResult> UpdateCODStatus([FromBody] UpdateCODStatusRequest request)
        {
            var order = await _context.DonHangs.FindAsync(request.MaDonHang);
            if (order == null) return NotFound();

            order.TrangThaiThuHo = request.TrangThaiThuHo;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class UpdateCODStatusRequest
    {
        public string MaDonHang { get; set; }
        public string TrangThaiThuHo { get; set; }
    }
}
