using Bookstore.Data.Entities;
using Bookstore.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(BookstoreContext db)
        {
            if (await db.Books.AnyAsync()) return;

            var tolkien = new Author { Name = "J.R.R. Tolkien", YearOfBirth = 1892 };
            var orwell = new Author { Name = "George Orwell", YearOfBirth = 1903 };
            var fantasy = new Genre { Name = "Fantasy" };
            var dystopia = new Genre { Name = "Dystopia" };

            var hobbit = new Book
            {
                Title = "The Hobbit",
                Price = 14.99m,
                Authors = { tolkien },
                Genres = { fantasy },
                Reviews = { new Review { Rating = 5, Text = "Great" },
                          new Review { Rating = 4, Text = "Good" } }
            };
            var nineteen84 = new Book
            {
                Title = "1984",
                Price = 9.99m,
                Authors = { orwell },
                Genres = { dystopia },
                Reviews = { new Review { Rating = 5, Text = "Classic" } }
            };

            db.Books.AddRange(hobbit, nineteen84);
            await db.SaveChangesAsync();
        }
    }
}
