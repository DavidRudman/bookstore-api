using Bookstore.Application.Abstractions;
using Bookstore.Application.Import;
using Bookstore.Data.Entities;
using Bookstore.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using FluentAssertions;

namespace Bookstore.Tests.Unit
{
    public class BookImportServiceTests
    {
        private static BookstoreContext NewInMemoryContext() =>
            new(new DbContextOptionsBuilder<BookstoreContext>()
                .UseInMemoryDatabase($"import-{Guid.NewGuid()}").Options);

        [Fact]
        public async Task Import_SkipsExistingTitles_CaseInsensitiveAndTrimmed()
        {
            await using var db = NewInMemoryContext();
            db.Books.Add(new Book { Title = "Existing Book" });
            await db.SaveChangesAsync();

            var external = Substitute.For<IExternalBookApi>();
            external.GetBooks().Returns(new List<Book>
          {
              new() { Title = "  existing book  " }, 
              new() { Title = "Brand New Book" } 
          });

            var sut = new BookImportService(db, external, NullLogger<BookImportService>.Instance);

            var result = await sut.ImportAsync();

            result.Imported.Should().Be(1);
            result.Skipped.Should().Be(1);
            (await db.Books.CountAsync()).Should().Be(2);
        }
    }
}
