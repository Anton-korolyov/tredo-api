using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tredo.Api.Data;

namespace Tredo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /api/categories
        [HttpGet]
        public IActionResult GetAll([FromQuery] string lang = "he")
        {
            var categories = _db.Categories
                .OrderBy(c => c.Id)
                .Select(c => new
                {
                    Id = c.Id,
                    Name =
                        lang == "ru" ? c.NameRu :
                        lang == "en" ? c.NameEn :
                        c.NameHe
                })
                .ToList();

            return Ok(categories);
        }
    }
}
