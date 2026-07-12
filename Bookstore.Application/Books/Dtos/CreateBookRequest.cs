
namespace Bookstore.Application.Books.Dtos
{
    public record CreateBookRequest(
        string Title,
        decimal Price,
        List<int> AuthorIds, // assuim author ids are available 
        List<int> GenreIds // assuming the genre ids are available
        );
    
}
