using Bookstore.Application.Books.Dtos;
using Bookstore.Application.Common.Validation;

namespace Bookstore.Application.Books.Validators
{
    public class CreateBookRequestValidator : IValidator<CreateBookRequest>
    {
        public IReadOnlyList<string> Validate(CreateBookRequest request)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                errors.Add("Title is required");
            }
            else if (request.Title.Length > 200) 
            {
                errors.Add("Title must be less than 200 characters.");
            }

            if (request.Price < 0)
            {
                errors.Add("Price must be >= 0");
            }

            //validate author exists

            //validate genre exists
            
            return errors;
        }   
    }
}
