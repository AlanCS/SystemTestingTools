using MovieProject.Logic.Extensions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace MovieProject.IsolatedTests.UnitTests
{
    public class StringExtensionsTests
    {
        [InlineData(null, "Unknown")]
        [InlineData(" ", "Unknown")]
        [InlineData("    ", "Unknown")]
        [InlineData("-1 min", "Unknown")]
        [InlineData("0 min", "Unknown")]
        [InlineData("1 min", "1 min")]
        [InlineData("59 min", "59 min")]
        [InlineData("60 gibberish", "Unknown")]
        [InlineData("136 min", "2.3h")]
        [InlineData("60 min", "1h")]
        [InlineData("43,200 min", "720h")] // longest movie ever: Ambiancé
        [Theory]
        public void CanFormatDuration(string durationInMinutes, string durationFormatted)
        {
            durationInMinutes.FormatDuration().ShouldBe(durationFormatted);
        }

        [InlineData(null, "Unknown")]
        [InlineData("", "Unknown")]
        [InlineData("    ", "Unknown")]
        [InlineData("gibberish", "Unknown")]
        [InlineData("0", "Unknown")]
        [InlineData("1887", "Unknown")]
        [InlineData("1888", "1888")] // year of the first movie ever made
        [InlineData("2019", "2019")]
        [InlineData("   2019   ", "2019")]
        [InlineData("3019", "Unknown")]
        [Theory]
        public void CanCleanYear(string yearString, string expectedResult)
        {
            yearString.CleanYear().ShouldBe(expectedResult);
        }

        [InlineData("", "")]
        [InlineData("    ", "")]
        [InlineData("   some title  ", "some title")]
        [InlineData("N/A", "")]
        [Theory]
        public void CanCleanNA(string originalString, string expectedResult)
        {
            originalString.CleanNA().ShouldBe(expectedResult);
        }
    }
}
