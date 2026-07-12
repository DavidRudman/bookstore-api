using Bookstore.Application.Books.Dtos;

namespace Bookstore.Application.Books
{
    public interface IBookService
    {
        Task<IReadOnlyList<BookDto>> GetBooksAsync(CancellationToken ct = default);
        Task<IReadOnlyList<BookDto>> GetTopBooksByRatingAsync(int count = 10, CancellationToken ct = default);
        Task<BookDto> CreateBookAsync(CreateBookRequest request, CancellationToken ct = default);
        Task<bool> UpdatePriceAsync(int id, decimal price, CancellationToken ct = default);
        Task<bool> DeleteBookAsync(int id, CancellationToken ct = default);
    }
}
