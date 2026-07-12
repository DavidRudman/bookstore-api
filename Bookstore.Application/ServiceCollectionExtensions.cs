using Bookstore.Application.Abstractions;
using Bookstore.Application.Books;
using Bookstore.Application.Books.Dtos;
using Bookstore.Application.Books.Validators;
using Bookstore.Application.Common.Validation;
using Bookstore.Application.Import;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IValidator<CreateBookRequest>, CreateBookRequestValidator>();
            services.AddScoped<IValidator<UpdateBookPriceRequest>, UpdatePriceRequestValidator>();
            services.AddScoped<IBookImporter, BookImportService>();
            services.AddScoped<IExternalBookApi, MockExternalBookApi>();
            return services;
        }
    }
}
