using System;

namespace MovieProject.Logic.Extensions
{
    public static class StringExtensions
    {
        public static string FormatDuration(this string durationInMinutes)
        {
            if (string.IsNullOrWhiteSpace(durationInMinutes)) return "Unknown";

            // we expect the format to be like '44 min'
            var parts = durationInMinutes.Split(new char[] { ' ' });
           
            if (parts.Length != 2) return "Unknown";
            if (parts[1] != "min") return "Unknown"; // we don't know what unit it is

            if(!int.TryParse(parts[0].Replace(",","").Replace(".", ""), out var minutes)) return "Unknown";

            if(minutes <= 0) return "Unknown";

            if (minutes <= 59) return $"{minutes} min";

            var hours = Math.Round(minutes / 60.0, 1);

            return $"{hours}h";
        }

        public static string CleanYear(this string yearString)
        {
            if (string.IsNullOrWhiteSpace(yearString)) return "Unknown";

            if(!int.TryParse(yearString, out var year)) return "Unknown";

            if(year < 1888) // the year of the first movie
                return "Unknown";

            if(year > DateTime.Now.Year +20) // sometimes the service returns in production movies
                return "Unknown";

            return year.ToString();
        }

        public static string CleanNA(this string originalString)
        {
            originalString = originalString.Trim();
            if (originalString == "N/A") originalString = "";
            return originalString;
        }
    }
}