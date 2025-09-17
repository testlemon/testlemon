using System.Text.RegularExpressions;

namespace Testlemon.Core.Validators.Helpers
{
    public class ValidatorDateParser
    {
        const string DATE_PATTERN = @"(\d+)([a-zA-Z]*)";

        public static DateTime? ParseExpectedDate(string input)
        {
            if (DateTime.TryParse(input, out DateTime date))
            {
                return date;
            }

            var match =  Regex.Match(input, DATE_PATTERN);
            if (match.Success)
            {
                // Capture the number and the unit from groups
                var number = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;

                return unit switch
                {
                    "" or "d" or "day" or "days" => DateTime.UtcNow.AddDays(number),
                    "w" or "week" or "weeks" => DateTime.UtcNow.AddDays(number * 7),
                    "m" or "month" or "months" => DateTime.UtcNow.AddMonths(number),
                    "y" or "year" or "years" => DateTime.UtcNow.AddYears(number),
                    _ => throw new ArgumentException($"Could not parse the date unit: {unit}"),
                };
            }

            return null;
        }
    }
}