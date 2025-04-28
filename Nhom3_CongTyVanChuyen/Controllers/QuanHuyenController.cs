using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  
using Nhom3_CongTyVanChuyen.Data;

[ApiController]
[Route("api/[controller]")]
public class QuanHuyenController : ControllerBase
{
    private readonly MyDbContext _context;

    public QuanHuyenController(MyDbContext context)
    {
        _context = context;
    }

    [HttpGet("{maTinhTP}")]
    public async Task<IActionResult> GetQuanHuyenByTinh(string maTinhTP)
    {
        var quanHuyenList = await _context.QuanHuyens
            .Where(qh => qh.MaTinhTP == maTinhTP)
            .Select(qh => new
            {
                qh.MaQuanHuyen,
                qh.TenQuanHuyen
            })
            .ToListAsync();

        return Ok(quanHuyenList);
    }
}
