using Bookstore.Application.Books.Dtos;
using Bookstore.Data.Entities;
using Bookstore.Data.Persistence;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Application.Books
{
    public class BookService : IBookService
    {
        private readonly BookstoreContext _db;
        private readonly string _connectionString;

        public BookService(BookstoreContext db, IConfiguration config)
        {
            _db = db;
            _connectionString = config.GetConnectionString("Default")!;
        }

        public async Task<IReadOnlyList<BookDto>> GetBooksAsync(CancellationToken ct = default)
        {
            return await _db.Books
                .AsNoTracking()
                .Select(x => new BookDto(
                    x.Id,
                    x.Title,
                    x.Price,
                    x.Authors.Count > 0 ? x.Authors.Select(y => y.Name).ToList() : new List<string>(),
                    x.Genres.Count > 0 ? x.Genres.Select(y => y.Name).ToList() : new List<string>(),
                    x.Reviews.Count > 0 ? x.Reviews.Average(y => y.Rating) : 0
                ))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<BookDto>> GetTopBooksByRatingAsync(int count = 10, CancellationToken ct = default)
        {
            string sqlCommand = @"
                SELECT TOP (@Count)
                    b.Id AS Id,
                    b.Title AS Title,
                    b.Price as Price,
                    COALESCE(AVG(CAST(r.Rating AS float)), 0) AS AverageRating,
                    (SELECT STRING_AGG(a.Name, '||')
                    FROM Authors a
                    JOIN AuthorBook ab ON ab.AuthorsId = a.Id
                    WHERE ab.BooksId = b.Id)  AS Authors,
                    (SELECT STRING_AGG(g.Name, '||')
                    FROM Genres g
                    JOIN BookGenre bg ON bg.GenresId = g.Id
                    WHERE bg.BooksId = b.Id)  AS Genres
                FROM Books b
                LEFT JOIN Reviews r ON r.BookId = b.Id
                GROUP BY b.Id, b.Title
                ORDER BY AverageRating DESC, b.Title ASC;";

            await using var connection = new SqlConnection(_connectionString);
            var command = new CommandDefinition(sqlCommand, new { Count = count }, cancellationToken: ct);
            var rows = await connection.QueryAsync<TopBooksRow>(command);

            return rows
                .Select(x => new BookDto
                (
                    x.Id,
                    x.Title,
                    x.Price,
                    Split(x.Authors),
                    Split(x.Genres),
                    x.AverageRating
                )).ToList();
        }

        public async Task<BookDto> CreateBookAsync(CreateBookRequest request, CancellationToken ct = default)
        {
            var authors = await _db.Authors.Where(x => request.AuthorIds.Contains(x.Id)).ToListAsync(ct);
            var genres = await _db.Genres.Where(x => request.GenreIds.Contains(x.Id)).ToListAsync(ct);

            var book = new Book
            {
                Title = request.Title.Trim(),
                Price = request.Price,
                Authors = authors,
                Genres = genres
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync(ct);

            return new BookDto(
                book.Id,
                book.Title,
                book.Price,
                authors.Select(x => x.Name).ToList(),
                genres.Select(x => x.Name).ToList(),
                0);
        }

        public async Task<bool> UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
        {
            var book = await _db.Books.FindAsync([id], ct);
            if (book is null) return false;
            book.Price = price;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteBookAsync(int id, CancellationToken ct = default)
        {
            var book = await _db.Books.FindAsync([id], ct);
            if (book is null) return false;
            _db.Books.Remove(book);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private static IReadOnlyList<string> Split(string? s) =>
            string.IsNullOrWhiteSpace(s) ? Array.Empty<string>() : s.Split("||");


        private sealed record TopBooksRow(int Id, string Title, decimal Price, double AverageRating, string? Authors, string? Genres);
    }
}
