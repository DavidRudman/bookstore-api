namespace Bookstore.Data.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int YearOfBirth { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
