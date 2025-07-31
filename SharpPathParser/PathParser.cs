// <copyright file="PathParser.cs" company="SharpPathParser contributors">
// Copyright (c) SharpPathParser contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SharpPathParser;

using System.Text.RegularExpressions;
using SharpPathParser.Enum;
using SharpPathParser.Extensions;

/// <summary>
/// A universal file parser for extracting string literals, dates, times, and arbitrary regexes.
/// </summary>
public class PathParser
{
    private Dictionary<string, IEnumerable<string>>? Groups { get; set; }

    private Dictionary<string, Regex> Regexes { get; set; }

    private bool Date { get; set; }

    private bool Time { get; set; }

    private char Separator { get; set; }

    private bool DeepSearch { get; set; }

    private PathParserPriority Priority { get; set; }

#pragma warning disable SA1124 // DoNotUseRegions
    #region Constructors
    public PathParser(
        bool date,
        bool time,
        char separator = '\0',
        PathParserPriority priority = PathParserPriority.Filename,
        Dictionary<string, Regex>? regexes = null,
        IEnumerable<string>? groups = null,
        bool deepSearch = false)
    {
        this.Date = date;
        this.Time = time;
        this.Separator = separator;
        this.Priority = priority;
        this.Regexes = regexes ?? new();
        this.DeepSearch = deepSearch;

        this.Groups = groups.Select((group, index) => new { Key = $"group{index}", Value = new[] { group }.AsEnumerable() })
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public PathParser(
        bool date,
        bool time,
        char separator = '\0',
        PathParserPriority priority = PathParserPriority.Filename,
        Dictionary<string, Regex>? regexes = null,
        IEnumerable<IEnumerable<string>>? groups = null,
        bool deepSearch = false)
    {
        this.Date = date;
        this.Time = time;
        this.Separator = separator;
        this.Priority = priority;
        this.Regexes = regexes ?? new();
        this.DeepSearch = deepSearch;

        if (groups != null)
        {
            this.Groups = groups.Select((group, index) => new { Key = $"group{index}", Value = group })
                .ToDictionary(x => x.Key, x => x.Value);
        }
        else
        {
            this.Groups = null;
        }
    }

    public PathParser(
        bool date,
        bool time,
        char separator = '\0',
        PathParserPriority priority = PathParserPriority.Filename,
        Dictionary<string, Regex>? regexes = null,
        Dictionary<string, string>? groups = null,
        bool deepSearch = false)
    {
        this.Date = date;
        this.Time = time;
        this.Separator = separator;
        this.Priority = priority;
        this.Regexes = regexes ?? new();
        this.DeepSearch = deepSearch;

        this.Groups = groups?.ToDictionary(
            x => x.Key,
            x => new[] { x.Value }.AsEnumerable());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathParser"/> class.
    /// Constructs a universal file parser for extracting string literals, dates, times, and arbitrary regexes.
    /// </summary>
    /// <param name="date">Whether to use the built-in date regexes.</param>
    /// <param name="time">Whether to use the built-in time regexes.</param>
    /// <param name="separator"></param>
    /// <param name="priority"></param>
    /// <param name="regexes"></param>
    /// <param name="groups"></param>
    /// <param name="deepSearch">Whether to check extensively for dates and times through multiple blocks (like dates as 2024/1/1)</param>
    public PathParser(
        bool date,
        bool time,
        char separator = '\0',
        PathParserPriority priority = PathParserPriority.Filename,
        Dictionary<string, Regex>? regexes = null,
        System.Enum? groups = null,
        bool deepSearch = false)
    {
        this.Date = date;
        this.Time = time;
        this.Separator = separator;
        this.Priority = priority;
        this.Regexes = regexes ?? new();
        this.DeepSearch = deepSearch;

        if (groups != null)
        {
            var enumNames = groups.GetType().GetEnumNames();
            var enumTypeName = groups.GetType().Name;
            Dictionary<string, IEnumerable<string>> groupsDictionary = new();
            groupsDictionary.Add(enumTypeName, enumNames);
            this.Groups = groupsDictionary;
        }
        else
        {
            this.Groups = null;
        }
    }

    public PathParser(
        bool date,
        bool time,
        char separator = '\0',
        PathParserPriority priority = PathParserPriority.Filename,
        Dictionary<string, Regex>? regexes = null,
        Dictionary<string, IEnumerable<string>>? groups = null,
        bool deepSearch = false)
    {
        this.Groups = groups;
        this.Date = date;
        this.Time = time;
        this.Separator = separator;
        this.Priority = priority;
        this.Regexes = regexes ?? new();
        this.DeepSearch = deepSearch;
    }
    #endregion
#pragma warning restore SA1124 // DoNotUseRegions

    public ParserResult Parse(string fullPath)
    {
        var candidatePath = this.Priority == PathParserPriority.Filename ? Path.GetFileName(fullPath) : fullPath;
        var blocks = this.SplitBlocks(candidatePath);

        var results = new ParserResult
        {
            Matches = PatternMatcher.MatchGroups(this.Groups, blocks),
            Time = this.Time ? PatternMatcher.MatchTime(blocks, this.DeepSearch) : null,
            Date = this.Date ? PatternMatcher.MatchDate(blocks, this.DeepSearch) : null,
        };

        results.Matches.AddRange(PatternMatcher.MatchRegexes(this.Regexes, blocks, this.DeepSearch));

        return results;
    }

    /// <summary>
    /// Splits a path into chunks to apply regexes to.
    /// </summary>
    /// <param name="fullPath">The full file path.</param>
    /// <returns>An array of strings split by a universal regex or by the user-defined separator.</returns>
    private string[] SplitBlocks(string fullPath)
    {
        var blockSplitRegex = new Regex(@"[\\\/\{\}\-_. \t]+");

        // Split using the given separator, otherwise use the default regex.
        return this.Separator == '\0' ? blockSplitRegex.Split(fullPath) : fullPath.Split(this.Separator);
    }
}