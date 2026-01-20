using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tredo.Api.Data;

namespace Tredo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CitiesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string search = "",
            string lang = "en"
        )
        {
            var query = _db.Cities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.NameEn.Contains(search) ||
                    c.NameRu.Contains(search) ||
                    c.NameHe.Contains(search)
                );
            }

            var raw = await query
                .OrderBy(c => c.NameEn)
                .Take(10)
                .Select(c => new
                {
                    c.Id,
                    c.NameEn,
                    c.NameRu,
                    c.NameHe
                })
                .ToListAsync();

            var result = raw.Select(c => new
            {
                c.Id,
                Name = lang switch
                {
                    "he" => c.NameHe,
                    "ru" => c.NameRu,
                    _ => c.NameEn
                }
            });

            return Ok(result);
        }
    }
}
