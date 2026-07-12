namespace Bookstore.Application.Books.Dtos
{
    public record BookDto(
        int Id,
        string Title,
        decimal Price,
        IReadOnlyList<string> Authors,
        IReadOnlyList<string> Genres,
        double AverageRating);
}
