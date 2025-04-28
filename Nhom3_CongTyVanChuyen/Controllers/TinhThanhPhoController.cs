using Microsoft.AspNetCore.Mvc;
using Nhom3_CongTyVanChuyen.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TinhThanhPhoController : ControllerBase
{
    private readonly MyDbContext _context;

    public TinhThanhPhoController(MyDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTinhThanhPho()
    {
        var list = await _context.TinhThanhPhos
            .Select(tp => new {
                tp.MaTinhTP,
                tp.TenTinhTP
            }).ToListAsync();

        return Ok(list);
    }

}
