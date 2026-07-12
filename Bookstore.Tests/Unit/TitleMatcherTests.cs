using Bookstore.Application.Import;
using FluentAssertions;

namespace Bookstore.Tests.Unit
{
    public class TitleMatcherTests
    {
        [Theory]
        [InlineData("Crime", "Crime", 0)]
        [InlineData("Crime", "Criem", 2)]
        [InlineData("abc", "abd", 1)]
        public void Levenshtein_ComputesEditDistance(string a, string b, int expected)
            => TitleMatcher.Levenshtein(a, b).Should().Be(expected);

        [Fact]
        public void IsSimilar_TreatsTypoAsSimilar()
            => TitleMatcher.IsSimilar("Crime and punishment", "Criem and punishment").Should().BeTrue();

        [Fact]
        public void Normalize_TrimsAndLowercases()
            => TitleMatcher.Normalize("  The Hobbit  ").Should().Be("the hobbit");
    }
}
