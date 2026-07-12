using Bookstore.Application.Books.Dtos;
using Bookstore.Application.Books.Validators;
using FluentAssertions;

namespace Bookstore.Tests.Unit
{
    public class UpdatePriceValidatorTests
    {
        private readonly UpdatePriceRequestValidator validator = new();

        [Fact]
        public void NegativePrice_ProducesError()
        {
            validator.Validate(new UpdateBookPriceRequest(-1m)).Should().NotBeEmpty();
        }

        [Fact]
        public void ValidPrice_ProducesNoErrors()
        {
            validator.Validate(new UpdateBookPriceRequest(9.99m)).Should().BeEmpty();
        }
    }
}
