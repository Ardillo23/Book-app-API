using Microsoft.AspNetCore.Mvc;
using VistaTiBooks.Api.Services;

namespace VistaTiBooks.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly OpenLibraryService _openLibrary;

        public BooksController(OpenLibraryService openLibrary)
        {
            _openLibrary = openLibrary;
        }

        // GET: /api/books/search?query=harry%20potter
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "query is required" });

            var result = await _openLibrary.SearchAsync(query, limit: 10);
            return Ok(result);
        }
    }
}
