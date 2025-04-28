using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhom3_CongTyVanChuyen.Data;

[ApiController]
[Route("api/[controller]")]
public class PhuongXaController : ControllerBase
{
    private readonly MyDbContext _context;

    public PhuongXaController(MyDbContext context)
    {
        _context = context;
    }

    [HttpGet("{maQuanHuyen}")]
    public async Task<IActionResult> GetPhuongXaByQuanHuyen(string maQuanHuyen)
    {
        var phuongXaList = await _context.PhuongXas
            .Where(px => px.MaQuanHuyen == maQuanHuyen)
            .Select(px => new
            {
                px.MaPhuongXa,
                px.TenPhuongXa
            })
            .ToListAsync();

        return Ok(phuongXaList);
    }
}
