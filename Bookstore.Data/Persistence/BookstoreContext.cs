using Bookstore.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace Bookstore.Data.Persistence
{
    public class BookstoreContext : DbContext
    {
        public BookstoreContext(DbContextOptions<BookstoreContext> options) : base(options) { }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookstoreContext).Assembly);
            /*
            // I prefer lower case colum names so this is something that I would do (or something similar) 
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()!.ToLowerInvariant());
                foreach (var prop in entity.GetProperties())
                {
                    prop.SetColumnName(prop.GetColumnName().ToLowerInvariant());
                }
            }
            */
        }
    }
}
