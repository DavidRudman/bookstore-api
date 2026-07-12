using Bookstore.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookstore.Data.Persistence.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("Title")
                .HasMaxLength(209);

            builder.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasColumnName("Price");
        }
    }
}
