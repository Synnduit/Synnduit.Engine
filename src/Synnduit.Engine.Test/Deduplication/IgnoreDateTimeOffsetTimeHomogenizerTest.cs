using System;
using Xunit;

namespace Synnduit.Deduplication
{
    public class IgnoreDateTimeOffsetTimeHomogenizerTest
    {
        [Fact]
        public void Trims_Time_Portion_Of_DateTimeOffset_Values()
        {
            HomogenizerTester.HomogenizesValuesOfSupportedType(
                new IgnoreDateTimeOffsetTimeHomogenizer(),
                homogenizedValue => homogenizedValue.DateTime,
                new[] {
                    new DateTimeOffset(1969, 7, 20, 14, 17, 40, TimeSpan.FromHours(-6)),
                    new DateTime(1969, 7, 20)
                },
                new[] { DateTimeOffset.Now, DateTime.Today },
                new[] {
                    new DateTimeOffset(1961, 4, 12, 12, 7, 7, TimeSpan.FromHours(6)),
                    new DateTime(1961, 4, 12)
                },
                new[] {
                    new DateTimeOffset(1972, 12, 11, 2, 55, 0, TimeSpan.FromHours(-5)),
                    new DateTime(1972, 12, 11)
                });
        }

        [Fact]
        public void Does_Not_Affect_Non_DateTimeOffset_Values()
        {
            HomogenizerTester.DoesNotAffectValuesOfUnsupportedTypes<
                IgnoreDateTimeOffsetTimeHomogenizer, DateTimeOffset>();
        }
    }
}
