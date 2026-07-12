using Bookstore.Application.Books;
using Bookstore.Application.Books.Dtos;
using Bookstore.Application.Import;
using Bookstore.PublicAPI.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.PublicAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.CanRead)]
    public class BooksController : ControllerBase
    {
        private readonly IBookService bookService;
        public BooksController(IBookService bookService)
        {
            this.bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BookDto>>> GetBooks(CancellationToken ct)
        {
            return Ok(await bookService.GetBooksAsync(ct));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IReadOnlyList<BookDto>>> GetTopTenBooksByRating(CancellationToken ct)
        {
            return Ok(await bookService.GetTopBooksByRatingAsync(10, ct));
        }

        [HttpPost]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<ActionResult<BookDto>> CreateBook(CreateBookRequest request, CancellationToken ct)
        {
            var created = await bookService.CreateBookAsync(request, ct);
            return CreatedAtAction(nameof(GetBooks), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}/[action]")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<IActionResult> UpdateBookPrice(int id, UpdateBookPriceRequest request, CancellationToken ct)
        {
            var ok = await bookService.UpdatePriceAsync(id, request.Price, ct);
            return ok ? Ok(200) : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<IActionResult> DeleteBook(int id, CancellationToken ct)
        {
            var ok = await bookService.DeleteBookAsync(id, ct);
            return ok ? Ok(200) : NotFound();
        }

        [HttpPost("/api/import")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<IActionResult> RunImport([FromServices] IBookImporter importer, CancellationToken ct)
        {
            return Ok(await importer.ImportAsync(ct));
        }

    }
}
