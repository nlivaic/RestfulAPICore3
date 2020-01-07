using System;

namespace API.Helpers
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateOfBirth, DateTimeOffset? dateOfDeath)
        {
            var finalYear = dateOfDeath.HasValue ? dateOfDeath.Value.Year : DateTime.UtcNow.Year;
            int age = finalYear - dateOfBirth.Year;
            return age;
        }
    }
}