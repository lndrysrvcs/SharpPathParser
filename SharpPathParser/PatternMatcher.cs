using System.Globalization;
using System.Text.RegularExpressions;

namespace SharpPathParser;

/// <summary>
/// A utility class to match common patterns and string literals.
/// </summary>
public static class PatternMatcher
{
    /// <summary>
    /// Matches all key-value groups to corresponding blocks, identifying similar patterns between them.
    /// </summary>
    /// <param name="groups">A dictionary containing groups as keys and their associated enumerable values.</param>
    /// <param name="blocks">An array of strings representing path segments to be matched against the groups.</param>
    /// <returns>A dictionary mapping group keys to their matched block values.</returns>
    public static Dictionary<string, string> MatchGroups(Dictionary<string, IEnumerable<string>>? groups, string[] blocks)
    {
        Dictionary<string, string> results = new();
        if (groups == null)
        {
            return results;
        }

        foreach (var block in blocks)
        {
            var lowerBlock = block.ToLowerInvariant();

            foreach (KeyValuePair<string, IEnumerable<string>> group in groups.Where(group => !results.ContainsKey(group.Key))
                         .Where(group => group.Value.Any(s => s.ToLowerInvariant() == lowerBlock)))
            {
                results.Add(group.Key, block);
                break; // Move to the next block once we find a match
            }
        }

        return results;
    }

    /// <summary>
    /// Identifies the first time pattern within the given blocks of text and returns it as a TimeOnly object.
    /// Optionally performs a deeper search using sliding window logic for more complex time formats.
    /// </summary>
    /// <param name="blocks">An array of strings representing blocks of text to search for time patterns.</param>
    /// <param name="deepSearch">Indicates whether to enable deep search by using a sliding window across the array of blocks.</param>
    /// <returns>A nullable TimeOnly object representing the first valid time pattern found, or null if no valid time is identified.</returns>
    public static TimeOnly? MatchTime(string[] blocks, bool deepSearch = false)
    {
        foreach (var block in blocks)
        {
            if (block.Length < 3)
            {
                continue; // Skip anything shorter than a time
            }

            if (TimeOnly.TryParse(block, out var time))
            {
                return time;
            }
        }

        if (!deepSearch)
        {
            return null;
        }

        // no luck iterating through blocks normally, use a sliding window on blocks to check
        for (var i = 0; i < blocks.Length - 3; i++)
        {
            var block1 = blocks[i];
            var block2 = blocks[i + 1];
            var block3 = blocks[i + 2];
            var block4 = blocks[i + 3];

            // 11:22:33 AM/PM
            if (TimeOnly.TryParse($"{block1}:{block2}:{block3} {block4}", out var timeExtMeridian))
            {
                return timeExtMeridian;
            }

            // 11:22 AM/PM
            if (TimeOnly.TryParse($"{block1}:{block2} {block3}", out var timeMeridian))
            {
                return timeMeridian;
            }

            // 11:22:33
            if (TimeOnly.TryParse($"{block1}:{block2}:{block3}", out var timeExt))
            {
                return timeExt;
            }

            // 11:22
            if (TimeOnly.TryParse($"{block1}:{block2}", out var time))
            {
                return time;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to identify and parse a date from the provided string array using basic or deep search logic.
    /// </summary>
    /// <param name="blocks">An array of strings to be analyzed for date patterns.</param>
    /// <param name="deepSearch">Indicates whether to enable deep search by using a sliding window across the array of blocks.</param>
    /// <returns>A parsed <see cref="DateOnly"/> object if a valid date is found; otherwise, null.</returns>
    public static DateOnly? MatchDate(string[] blocks, bool deepSearch = false)
    {
        foreach (var block in blocks)
        {
            if (block.Length < 4)
            {
                continue; // Skip anything shorter than a date
            }

            if (!TryFuzzyParseToDateOnly(block, out var date))
            {
                continue;
            }

            return date;
        }

        if (!deepSearch)
        {
            return null;
        }

        // no luck iterating through blocks normally, use a sliding window on blocks to check
        for (var i = 0; i < blocks.Length - 2; i++)
        {
            var block1 = blocks[i];
            var block2 = blocks[i + 1];
            var block3 = blocks[i + 2];

            if (block1.Length < 2 || block2.Length < 2 || block3.Length < 2)
            {
                continue; // Skip anything shorter than a time
            }

            if (DateOnly.TryParse($"{block1}-{block2}-{block3}", out var timeExt))
            {
                return timeExt;
            }

            if (DateOnly.TryParse($"{block1}-{block2}", out var time))
            {
                return time;
            }
        }

        return null;
    }

    /// <summary>
    /// Matches a collection of regex patterns against corresponding string blocks and retrieves the matched values.
    /// </summary>
    /// <param name="regexes">A dictionary of string keys and their associated regex patterns to be matched.</param>
    /// <param name="blocks">An array of strings to be checked against the regex patterns.</param>
    /// <param name="deepSearch">A boolean indicating whether to perform a deep search on the deepSearchText.</param>
    /// <param name="deepSearchText">A string to be searched using regex patterns if deep search is enabled.</param>
    /// <returns>A dictionary mapping regex keys to their matched block or deep search values.</returns>
    public static Dictionary<string, string> MatchRegexes(Dictionary<string, Regex> regexes, string[] blocks, bool deepSearch = false, string deepSearchText = "")
    {
        Dictionary<string, string> results = new();
        foreach (var block in blocks)
        {
            foreach (KeyValuePair<string, Regex> regex in regexes.Where(regex => !results.ContainsKey(regex.Key) && regex.Value.IsMatch(block)))
            {
                results[regex.Key] = block;
            }
        }

        if (deepSearch)
        {
            foreach (KeyValuePair<string, Regex> regex in regexes.Where(regex => regex.Value.IsMatch(deepSearchText)))
            {
                var match = regex.Value.Match(deepSearchText);
                results[regex.Key] = match.Groups[0].Value;
            }
        }

        return results;
    }

    /// <summary>
    /// Normalizes a given date string by replacing common separators (e.g., hyphens or dots) with slashes and trimming any surrounding whitespace.
    /// </summary>
    /// <param name="input">The input date string to be normalized.</param>
    /// <returns>A string with normalized separators and no surrounding whitespace.</returns>
    private static string NormalizeDateString(string input)
    {
        // Replace common separators with a consistent one (e.g., slash)
        return Regex.Replace(input.Trim(), @"[-\.]", "/");
    }

    /// <summary>
    /// Attempts to parse a string representation of a date into a <see cref="DateOnly"/> object, accommodating a variety of formats and separators.
    /// </summary>
    /// <param name="input">The input string potentially containing a date.</param>
    /// <param name="result">When the method returns, contains the parsed <see cref="DateOnly"/> value if parsing was successful; otherwise, the default value.</param>
    /// <returns>True if the input string was successfully parsed into a <see cref="DateOnly"/>; otherwise, false.</returns>
    private static bool TryFuzzyParseToDateOnly(string input, out DateOnly result)
    {
        input = NormalizeDateString(input);

        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
        {
            result = DateOnly.FromDateTime(dt);
            return true;
        }

        // Fallback to custom formats if needed
        string[] formats =
        [
            "yyMMdd",
            "yyyyMMdd",
            "ddMMyyyy",
            "MMddyy",
            "MMyydd",
            "ddMMyy"
        ];
        if (formats.Any(format => DateTime.TryParseExact(
                input,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt)))
        {
            result = DateOnly.FromDateTime(dt);
            return true;
        }

        result = default;
        return false;
    }
}