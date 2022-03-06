using Xunit;

namespace Synnduit.Deduplication
{
    public class CaseInsensitiveComparisonHomogenizerTest
    {
        [Fact]
        public void Converts_Strings_To_Upper_Case()
        {
            HomogenizerTester.HomogenizesValuesOfSupportedType(
                new CaseInsensitiveComparisonHomogenizer(),
                new[] { "One", "ONE" },
                new[] { "  twO ", "  TWO " },
                new[] { "  three  ", "  THREE  " },
                new[] { "four  ", "FOUR  " },
                new[] { "  FIVE", "  FIVE" },
                new[] { string.Empty, string.Empty },
                new[] { "  ", "  " },
                new[] { (string) null, null });
        }

        [Fact]
        public void Does_Not_Affect_Non_String_Values()
        {
            HomogenizerTester.DoesNotAffectValuesOfUnsupportedTypes<
                CaseInsensitiveComparisonHomogenizer, string>();
        }
    }
}
