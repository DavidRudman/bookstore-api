using Bookstore.Data.Entities;

namespace Bookstore.Application.Abstractions
{
    public interface IExternalBookApi
    {
        IReadOnlyList<Book> GetBooks();
    }
}
