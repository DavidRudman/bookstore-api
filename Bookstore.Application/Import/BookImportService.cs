using Bookstore.Application.Abstractions;
using Bookstore.Data.Entities;
using Bookstore.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Import
{
    public record ImportResult(int Imported, int Skipped);

    public interface IBookImporter
    {
        Task<ImportResult> ImportAsync(CancellationToken ct = default);
    }

    public class BookImportService : IBookImporter
    {
        private const int BatchSize = 2000;

        private readonly BookstoreContext databaseService;
        private readonly IExternalBookApi externalApi;
        private readonly ILogger<BookImportService> logger;

        public BookImportService(BookstoreContext databaseService, IExternalBookApi externalApi, ILogger<BookImportService> logger)
        {
            this.databaseService = databaseService;
            this.externalApi = externalApi;
            this.logger = logger;
        }

        public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
        {
            var incoming = externalApi.GetBooks();
            logger.LogInformation("Import started: {Count} books received", incoming.Count);

            var seenTitles = new HashSet<string>(
                (await databaseService.Books.Select(b => b.Title).ToListAsync(ct)).Select(t => t.Trim()),
                StringComparer.OrdinalIgnoreCase);

            //reuse existing authosr/genres by name so we dont insert duplicates
            var authorCache = await databaseService.Authors.ToDictionaryAsync(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase, ct);
            var genreCache = await databaseService.Genres.ToDictionaryAsync(g => g.Name, g => g, StringComparer.OrdinalIgnoreCase, ct);

            int imported = 0, skipped = 0;
            var batch = new List<Book>(BatchSize);

            foreach (var src in incoming)
            {
                ct.ThrowIfCancellationRequested();
                var title = src.Title.Trim();

                if (!seenTitles.Add(title)) { skipped++; continue; }

                batch.Add(new Book
                {
                    Title = title,
                    Price = src.Price,
                    Authors = src.Authors.Select(a => Resolve(authorCache, a.Name, () => new Author { Name = a.Name.Trim(), YearOfBirth = a.YearOfBirth })).ToList(),
                    Genres = src.Genres.Select(g => Resolve(genreCache, g.Name, () => new Genre { Name = g.Name.Trim() })).ToList()
                });
                imported++;

                if (batch.Count >= BatchSize)
                    await FlushAsync(batch, ct);
            }

            if (batch.Count > 0)
                await FlushAsync(batch, ct);

            logger.LogInformation("Import complete: {Imported} imported, {Skipped} skipped", imported, skipped);
            return new ImportResult(imported, skipped);
        }

        private async Task FlushAsync(List<Book> batch, CancellationToken ct)
        {
            databaseService.Books.AddRange(batch);
            await databaseService.SaveChangesAsync(ct);

            // Detach saved books to keep memory flat across 100k rows.
            // (Authors/genres stay tracked so the caches remain valid for the next batch.)
            foreach (var b in batch) databaseService.Entry(b).State = EntityState.Detached;
            batch.Clear();
        }

        private static T Resolve<T>(Dictionary<string, T> cache, string name, Func<T> create)
        {
            name = name.Trim();
            if (cache.TryGetValue(name, out var existing)) return existing;
            var created = create();
            cache[name] = created;
            return created;
        }

        // NOTE for production (real case): there should be a bulk action for this
    }
}
