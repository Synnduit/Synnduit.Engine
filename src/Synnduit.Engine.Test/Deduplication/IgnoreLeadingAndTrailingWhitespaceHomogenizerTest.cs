using Xunit;

namespace Synnduit.Deduplication
{
    public class IgnoreLeadingAndTrailingWhitespaceHomogenizerTest
    {
        [Fact]
        public void Trims_String_Values()
        {
            HomogenizerTester.HomogenizesValuesOfSupportedType(
                new IgnoreLeadingAndTrailingWhitespaceHomogenizer(),
                new[] { "    Hello  ", "Hello" },
                new[] { "\r\nout there", "out there" },
                new[] { "we're on\t", "we're on" },
                new[] { "the aiR", "the aiR" },
                new[] { "\t\t it's hockey\t\r\n   ", "it's hockey" },
                new[] { " night Tonight\r\n", "night Tonight" });
        }

        [Fact]
        public void Does_Not_Affect_Non_String_Values()
        {
            HomogenizerTester.DoesNotAffectValuesOfUnsupportedTypes<
                IgnoreLeadingAndTrailingWhitespaceHomogenizer, string>();
        }
    }
}
