using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VistaTiBooks.Api.Data;
using VistaTiBooks.Api.DTOs;
using VistaTiBooks.Api.Models;

namespace VistaTiBooks.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int FixedUserId = 1;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Favorites  -> "Mis favoritos"
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == FixedUserId)
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return Ok(favorites);
        }

        // POST: api/Favorites -> agrega favorito
        [HttpPost]
        public async Task<ActionResult<Favorite>> PostFavorite([FromBody] CreateFavoriteDto dto)
        {
            // validación mínima
            if (string.IsNullOrWhiteSpace(dto.ExternalId) || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "ExternalId y Title son obligatorios." });

            // evitar duplicados
            var exists = await _context.Favorites.AnyAsync(f =>
                f.UserId == FixedUserId && f.ExternalId == dto.ExternalId);

            if (exists)
                return Conflict(new { message = "Este libro ya está en favoritos." });

            var favorite = new Favorite
            {
                UserId = FixedUserId,
                ExternalId = dto.ExternalId.Trim(),
                Title = dto.Title.Trim(),
                Authors = string.IsNullOrWhiteSpace(dto.Authors) ? "Desconocido" : dto.Authors.Trim(),
                FirstPublishYear = dto.FirstPublishYear,
                CoverUrl = dto.CoverUrl
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(favorite);
        }

        // DELETE: api/Favorites/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == FixedUserId);

            if (favorite == null)
                return NotFound(new { message = $"Favorite with id={id} not found." });

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
