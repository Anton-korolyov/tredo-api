using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tredo.Api.Contracts;
using Tredo.Api.Data;
using Tredo.Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
namespace Tredo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IDistributedCache _cache;

        public CardsController(AppDbContext db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // =====================================================
        // GET: api/cards
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int? cityId,
            [FromQuery] string lang = "he",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12
        )
        {
            // ================= CACHE VERSION =================
            var versionStr = await _cache.GetStringAsync("cards:version");
            if (!int.TryParse(versionStr, out var version) || version < 1)
                version = 1;

            // ================= CACHE KEY =================
            var cacheKey =
                $"cards:v{version}:" +
                $"s={search}|" +
                $"cat={categoryId}|" +
                $"city={cityId}|" +
                $"lang={lang}|" +
                $"p={page}|" +
                $"ps={pageSize}";

            // ================= TRY REDIS =================
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return Ok(JsonSerializer.Deserialize<object>(cached, JsonOptions)!);

            // ================= QUERY =================
            var query = _db.Cards
                .Include(c => c.Category)
                .Include(c => c.City)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    EF.Functions.ILike(c.Title, $"%{search}%"));
            }

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (cityId.HasValue)
                query = query.Where(c => c.CityId == cityId.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CardResponse
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    Image = c.Image,
                    Description = c.Description,
                    Phone = c.Phone,

                    CategoryId = c.CategoryId,
                    CategoryName =
                        lang == "ru" ? c.Category.NameRu :
                        lang == "en" ? c.Category.NameEn :
                        c.Category.NameHe,

                    CityId = c.CityId,
                    CityName =
                        lang == "ru" ? c.City.NameRu :
                        lang == "en" ? c.City.NameEn :
                        c.City.NameHe,

                    OwnerId = c.OwnerId
                })
                .ToListAsync();

            var result = new
            {
                total,
                page,
                pageSize,
                items
            };

            // ================= SAVE REDIS =================
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result, JsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

            return Ok(result);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCardRequest req)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!await _db.Categories.AnyAsync(c => c.Id == req.CategoryId))
                return BadRequest("Invalid category");

            if (!await _db.Cities.AnyAsync(c => c.Id == req.CityId))
                return BadRequest("Invalid city");

            var card = new Card
            {
                Title = req.Title,
                Price = req.Price,
                Description = req.Description,
                Phone = req.Phone,
                CategoryId = req.CategoryId,
                CityId = req.CityId,
                OwnerId = userId
            };

            if (req.Image != null)
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "cards");

                Directory.CreateDirectory(path);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(req.Image.FileName)}";
                var fullPath = Path.Combine(path, fileName);

                using var stream = System.IO.File.Create(fullPath);
                await req.Image.CopyToAsync(stream);

                card.Image = $"/api/uploads/cards/{fileName}";
            }

            _db.Cards.Add(card);
            await _db.SaveChangesAsync();

            await IncrementCardsVersion();

            return Ok(await BuildResponse(card.Id));
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromForm] CardFormRequest req,
            [FromQuery] string lang = "he")
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var card = await _db.Cards.FindAsync(id);
            if (card == null) return NotFound();
            if (card.OwnerId != userId) return Forbid();

            card.Title = req.Title;
            card.Price = req.Price;
            card.Description = req.Description;
            card.Phone = req.Phone;
            card.CategoryId = req.CategoryId;
            card.CityId = req.CityId;

            if (req.Image != null)
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot", "uploads", "cards");

                Directory.CreateDirectory(path);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(req.Image.FileName)}";
                var fullPath = Path.Combine(path, fileName);

                using var stream = System.IO.File.Create(fullPath);
                await req.Image.CopyToAsync(stream);

                card.Image = $"/api/uploads/cards/{fileName}";
            }

            await _db.SaveChangesAsync();
            await IncrementCardsVersion();

            return Ok(await BuildResponse(card.Id, lang));
        }

        // =====================================================
        // DELETE
        // =====================================================
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var card = await _db.Cards.FindAsync(id);
            if (card == null) return NotFound();
            if (card.OwnerId != userId) return Forbid();

            _db.Cards.Remove(card);
            await _db.SaveChangesAsync();

            await IncrementCardsVersion();

            return NoContent();
        }

        // =====================================================
        // MY CARDS
        // =====================================================
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyCards()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var items = await _db.Cards
                .Where(c => c.OwnerId == userId)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Price,
                    c.Image
                })
                .ToListAsync();

            return Ok(items);
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private async Task<CardResponse> BuildResponse(Guid id, string lang = "he")
        {
            return await _db.Cards
                .Include(c => c.Category)
                .Include(c => c.City)
                .Where(c => c.Id == id)
                .Select(c => new CardResponse
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    Image = c.Image,
                    Description = c.Description,
                    Phone = c.Phone,

                    CategoryId = c.CategoryId,
                    CategoryName =
                        lang == "ru" ? c.Category.NameRu :
                        lang == "en" ? c.Category.NameEn :
                        c.Category.NameHe,

                    CityId = c.CityId,
                    CityName =
                        lang == "ru" ? c.City.NameRu :
                        lang == "en" ? c.City.NameEn :
                        c.City.NameHe,

                    OwnerId = c.OwnerId
                })
                .FirstAsync();
        }

        private async Task IncrementCardsVersion()
        {
            var current = await _cache.GetStringAsync("cards:version");

            if (!int.TryParse(current, out var version))
                version = 1;

            await _cache.SetStringAsync("cards:version", (version + 1).ToString());
        }
        private static readonly JsonSerializerOptions JsonOptions =
           new()
           {
               PropertyNamingPolicy = JsonNamingPolicy.CamelCase
           };
    }
}

