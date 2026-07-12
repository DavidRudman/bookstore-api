using Bookstore.Application.Books.Dtos;
using Bookstore.Application.Common.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Application.Books.Validators
{
    public class UpdatePriceRequestValidator : IValidator<UpdateBookPriceRequest>
    {
        public IReadOnlyList<string> Validate(UpdateBookPriceRequest request)
        {
            List<string> errors = new List<string>();

            if (request.Price < 0)
            {
                errors.Add("Price must be >= 0.");
            }

            return errors;
        }
    }
}
