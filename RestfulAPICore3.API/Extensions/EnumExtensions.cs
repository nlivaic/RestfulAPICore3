using System;
using System.Linq;

namespace API.Extensions
{
    public static class EnumUtils
    {
        public static bool Contains<T>(string target, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
            => Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .Select(e => e.ToString())
                .Any(e => e.Equals(target, comparisonType));
    }
}