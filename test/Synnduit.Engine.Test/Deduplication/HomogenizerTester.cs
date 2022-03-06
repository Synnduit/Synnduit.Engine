using FluentAssertions;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Synnduit.Deduplication
{
    internal static class HomogenizerTester
    {
        private static object[] ControlValues =
        {
            DateTime.Now,
            27887,
            "What Do We Do Now ",
            "  Little You",
            873385.8387d,
            83874L,
            DateTimeOffset.Now,
            DateTime.Today,
            883743.34m,
            new ArgumentOutOfRangeException(),
            new object(),
            new object[4],
            new[] { 1, 1, 2, 3, 5, 8 },
            null,
            'a',
            "  ...born and raised in South Detroit...  ",
            'C'
        };

        public static void HomogenizesValuesOfSupportedType<THomogenizer, TValue>(
            THomogenizer homogenizer,
            Func<TValue, object> convertHomogenizedValue,
            params TValue[][] valuePairs)
            where THomogenizer : Homogenizer<TValue>
        {
            foreach(TValue[] valuePair in valuePairs)
            {
                homogenizer
                    .Homogenize(valuePair[0])
                    .Should()
                    .Be(convertHomogenizedValue(valuePair[1]));
            }
        }

        public static void HomogenizesValuesOfSupportedType<THomogenizer, TValue>(
            THomogenizer homogenizer,
            params TValue[][] valuePairs)
            where THomogenizer : Homogenizer<TValue>
        {
            HomogenizesValuesOfSupportedType(
                homogenizer,
                homogenizedValue => homogenizedValue,
                valuePairs);
        }

        public static void DoesNotAffectValuesOfUnsupportedTypes<THomogenizer, TValue>()
            where THomogenizer : Homogenizer<TValue>
        {
            var homogenizer = (THomogenizer)
                FormatterServices.GetSafeUninitializedObject(typeof(THomogenizer));
            foreach(object controlValue in
                ControlValues
                .Where(cv => cv == null || cv.GetType() != typeof(TValue)))
            {
                homogenizer
                    .Homogenize(controlValue)
                    .Should()
                    .BeSameAs(controlValue);
            }
        }
    }
}
