using Bookstore.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Bookstore.Data.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.Property(e => e.Text).HasMaxLength(2000);

            builder.ToTable(e => e.HasCheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5"));

            builder.HasOne(e => e.Book)
                .WithMany(e => e.Reviews)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
