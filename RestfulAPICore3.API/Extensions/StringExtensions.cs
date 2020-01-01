using System.Linq;

namespace API.Extensions
{
    public static class StringExtensions
    {
        public static string CapitalizeFirstLetter(this string str) =>
            str.Trim().Length == 0
                ? str
                : str.First().ToString().ToUpper() + str.Substring(1);
    }
}