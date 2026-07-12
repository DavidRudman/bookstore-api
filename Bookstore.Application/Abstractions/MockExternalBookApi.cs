using Bookstore.Data.Entities;

namespace Bookstore.Application.Abstractions
{
    public class MockExternalBookApi : IExternalBookApi
    {
        public IReadOnlyList<Book> GetBooks()
        {
            var genres = new[] { "Fantasy", "Sci-Fi", "Mystery", "Romance", "History" };
            var books = new List<Book>(100_000);

            for (int i = 0; i < 100_000; i++)
            {
                books.Add(new Book
                {
                    Title = $"Imported Book {i}",
                    Price = 9.99m + (i % 20),
                    Authors = { new Author { Name = $"Author {i % 5000}", YearOfBirth = 1900 + (i % 100) } },
                    Genres = { new Genre { Name = genres[i % genres.Length] } }
                });
            }
            return books;
        }
    }
}
