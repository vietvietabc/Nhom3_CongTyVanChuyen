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
    public class HangHoaController2 : ControllerBase
    {
        private readonly MyDbContext _context;

        public HangHoaController2(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/HangHoaController2
        [HttpGet]
        public async Task<ActionResult> GetAllHangHoa()
        {
            try
            {
                var hangHoas = await _context.HangHoas
                    .Include(hh => hh.DanhMuc)
                    .Select(hh => new
                    {
                        hh.MaHangHoa,
                        hh.MaDanhMuc,
                        TenDanhMuc = hh.DanhMuc.TenDanhMuc,
                        hh.TinhChatHangHoa,
                        hh.DonGia
                    })
                    .ToListAsync();

                return Ok(hangHoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách hàng hóa: " + ex.Message });
            }
        }

        // GET: api/HangHoaController2/ByDanhMuc/{maDanhMuc}
        [HttpGet("ByDanhMuc/{maDanhMuc}")]
        public async Task<ActionResult> GetHangHoaByDanhMuc(string maDanhMuc)
        {
            try
            {
                var hangHoas = await _context.HangHoas
                    .Where(hh => hh.MaDanhMuc == maDanhMuc)
                    .Include(hh => hh.DanhMuc)
                    .Select(hh => new
                    {
                        hh.MaHangHoa,
                        hh.MaDanhMuc,
                        TenDanhMuc = hh.DanhMuc.TenDanhMuc,
                        hh.TinhChatHangHoa,
                        hh.DonGia
                    })
                    .ToListAsync();

                return Ok(hangHoas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách hàng hóa theo danh mục: " + ex.Message });
            }
        }

        // GET: api/HangHoaController2/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetHangHoaById(string id)
        {
            try
            {
                var hangHoa = await _context.HangHoas
                    .Include(hh => hh.DanhMuc)
                    .Where(hh => hh.MaHangHoa == id)
                    .Select(hh => new
                    {
                        hh.MaHangHoa,
                        hh.MaDanhMuc,
                        TenDanhMuc = hh.DanhMuc.TenDanhMuc,
                        hh.TinhChatHangHoa,
                        hh.DonGia
                    })
                    .FirstOrDefaultAsync();

                if (hangHoa == null)
                    return NotFound(new { message = "Không tìm thấy hàng hóa" });

                return Ok(hangHoa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin hàng hóa: " + ex.Message });
            }
        }

        // GET: api/HangHoaController2/TinhChatDacBiet/{maDanhMuc}
        [HttpGet("TinhChatDacBiet/{maDanhMuc}")]
        public async Task<ActionResult> GetTinhChatDacBietByDanhMuc(string maDanhMuc)
        {
            try
            {
                // Get all products in this category that have characteristics
                var hangHoasWithCharacteristics = await _context.HangHoas
                    .Where(hh => hh.MaDanhMuc == maDanhMuc && !string.IsNullOrEmpty(hh.TinhChatHangHoa))
                    .Select(hh => hh.TinhChatHangHoa)
                    .ToListAsync();

                // Parse and collect unique characteristics
                var uniqueCharacteristics = new HashSet<string>();
                
                foreach (var tinhChat in hangHoasWithCharacteristics)
                {
                    if (!string.IsNullOrEmpty(tinhChat))
                    {
                        // Split by common delimiters and clean up
                        var characteristics = tinhChat.Split(new char[] { ',', ';', '|', '\n', '\r' }, 
                            StringSplitOptions.RemoveEmptyEntries);
                        
                        foreach (var characteristic in characteristics)
                        {
                            var cleaned = characteristic.Trim();
                            if (!string.IsNullOrEmpty(cleaned))
                            {
                                uniqueCharacteristics.Add(cleaned);
                            }
                        }
                    }
                }

                var result = uniqueCharacteristics.OrderBy(x => x).ToList();
                
                // If no characteristics found in database, return empty list
                if (result.Count == 0)
                {
                    return Ok(new List<string>());
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy tính chất đặc biệt: " + ex.Message });
            }
        }

        // POST: api/HangHoaController2/Create
        [HttpPost("Create")]
        public async Task<ActionResult> CreateHangHoa([FromBody] HangHoaCreateDTO hangHoaDTO)
        {
            try
            {
                if (hangHoaDTO == null)
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });

                // Validate required fields
                if (string.IsNullOrEmpty(hangHoaDTO.MaDanhMuc))
                {
                    return BadRequest(new { message = "Thiếu mã danh mục" });
                }

                // Check if category exists
                var danhMuc = await _context.DanhMucs.FindAsync(hangHoaDTO.MaDanhMuc);
                if (danhMuc == null)
                    return BadRequest(new { message = "Danh mục không tồn tại" });

                // Generate product ID
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

                // Create product
                var hangHoa = new HangHoa
                {
                    MaHangHoa = maHangHoa,
                    MaDanhMuc = hangHoaDTO.MaDanhMuc,
                    TinhChatHangHoa = hangHoaDTO.TinhChatHangHoa,
                    DonGia = hangHoaDTO.DonGia
                };

                _context.HangHoas.Add(hangHoa);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetHangHoaById), new { id = maHangHoa }, new
                {
                    maHangHoa = hangHoa.MaHangHoa,
                    message = "Tạo hàng hóa thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo hàng hóa: " + ex.Message });
            }
        }

        // PUT: api/HangHoaController2/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateHangHoa(string id, [FromBody] HangHoaUpdateDTO hangHoaDTO)
        {
            try
            {
                var hangHoa = await _context.HangHoas.FindAsync(id);
                if (hangHoa == null)
                    return NotFound(new { message = "Không tìm thấy hàng hóa" });

                if (!string.IsNullOrEmpty(hangHoaDTO.MaDanhMuc))
                {
                    var danhMuc = await _context.DanhMucs.FindAsync(hangHoaDTO.MaDanhMuc);
                    if (danhMuc == null)
                        return BadRequest(new { message = "Danh mục không tồn tại" });
                    
                    hangHoa.MaDanhMuc = hangHoaDTO.MaDanhMuc;
                }

                if (!string.IsNullOrEmpty(hangHoaDTO.TinhChatHangHoa))
                    hangHoa.TinhChatHangHoa = hangHoaDTO.TinhChatHangHoa;

                if (hangHoaDTO.DonGia.HasValue)
                    hangHoa.DonGia = hangHoaDTO.DonGia.Value;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật hàng hóa thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật hàng hóa: " + ex.Message });
            }
        }

        // DELETE: api/HangHoaController2/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteHangHoa(string id)
        {
            try
            {
                var hangHoa = await _context.HangHoas.FindAsync(id);
                if (hangHoa == null)
                    return NotFound(new { message = "Không tìm thấy hàng hóa" });

                _context.HangHoas.Remove(hangHoa);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa hàng hóa thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa hàng hóa: " + ex.Message });
            }
        }
        // GET: api/HangHoaController2/GetPriceByCharacteristic
        [HttpGet("GetPriceByCharacteristic")]
        public async Task<ActionResult<double>> GetPriceByCharacteristic(string characteristic)
        {
            try
            {
                if (string.IsNullOrEmpty(characteristic))
                    return Ok(0);

                // Tìm hàng hóa có tính chất này và lấy đơn giá
                var hangHoa = await _context.HangHoas
                    .Where(hh => hh.TinhChatHangHoa != null &&
                                hh.TinhChatHangHoa.Contains(characteristic))
                    .FirstOrDefaultAsync();

                if (hangHoa != null)
                {
                    return Ok(hangHoa.DonGia);
                }

                return Ok(0);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy giá tính chất đặc biệt: " + ex.Message });
            }
        }

        // DTO classes
        public class HangHoaCreateDTO
        {
            public string MaDanhMuc { get; set; }
            public string TinhChatHangHoa { get; set; }
            public double DonGia { get; set; }
        }

        public class HangHoaUpdateDTO
        {
            public string MaDanhMuc { get; set; }
            public string TinhChatHangHoa { get; set; }
            public double? DonGia { get; set; }
        }
    }
}
