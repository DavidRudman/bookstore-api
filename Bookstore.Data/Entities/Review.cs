namespace Bookstore.Data.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? Text { get; set; }

        public virtual Book? Book { get; set; }
    }
}
