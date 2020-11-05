using MovieProject.Logic.DTO;
using System;
using System.Text.RegularExpressions;

namespace MovieProject.Logic.Extensions
{
    public static class StringExtensions
    {
        public static string FormatDuration(this string durationInMinutes)
        {
            if (string.IsNullOrWhiteSpace(durationInMinutes)) return Constants.Unknown;

            // we expect the format to be like '44 min'
            var parts = durationInMinutes.Split(new char[] { ' ' });
           
            if (parts.Length != 2) return Constants.Unknown;
            if (parts[1] != "min") return Constants.Unknown; // we don't know what unit it is

            if(!int.TryParse(parts[0].Replace(",","").Replace(".", ""), out var minutes)) return Constants.Unknown;

            if(minutes <= 0) return Constants.Unknown;

            if (minutes <= 59) return $"{minutes} min";

            var hours = Math.Round(minutes / 60.0, 1);

            return $"{hours}h";
        }

        // example 1998-2005
        private static readonly Regex seriesYearsRegex = new Regex("^(19|20)[0-9]{2}–(19|20)[0-9]{2}$", RegexOptions.Compiled);
        public static string CleanYear(this string yearString)
        {
            if (string.IsNullOrWhiteSpace(yearString)) return Constants.Unknown;

            yearString = yearString.Trim();
            if (seriesYearsRegex.IsMatch(yearString))
            {
                yearString = yearString.Replace("-", " to ").Replace("–", " to ");
                return yearString;
            }

            if (!int.TryParse(yearString, out var year)) return Constants.Unknown;

            if(year < 1888) // the year of the first movie
                return Constants.Unknown;

            if(year > DateTime.Now.Year +20) // sometimes the service returns in production movies
                return Constants.Unknown;            

            return year.ToString();
        }

        public static string CleanNA(this string originalString)
        {
            originalString = originalString.Trim();
            if (originalString == "N/A") originalString = "";
            return originalString;
        }

        public static Media.Languages GetLanguage(this string languageString)
        {
            if(string.IsNullOrWhiteSpace(languageString)) return Media.Languages.Other;

            languageString = languageString.ToLower();
            if (languageString.Contains("english")) return Media.Languages.English;
            if (languageString.Contains("french") || languageString.StartsWith("espa")) return Media.Languages.Spanish;
            if (languageString.Contains("spanish") || languageString.StartsWith("français")) return Media.Languages.French;

            return Media.Languages.Other;
        }
    }
}