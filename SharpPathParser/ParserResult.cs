// <copyright file="ParserResult.cs" company="SharpPathParser contributors">
// Copyright (c) SharpPathParser contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SharpPathParser;
using Extensions;

/// <summary>
/// The structured result from the parser parsing a file path.
/// </summary>
public record ParserResult
{
    /// <summary>
    /// Gets or sets the time extracted from parsing a file path.
    /// Represents a nullable <see cref="TimeOnly"/> object which holds time information
    /// if successfully parsed; otherwise, null.
    /// </summary>
    public TimeOnly? Time { get; set; }

    /// <summary>
    /// Gets or sets the date extracted from parsing a file path.
    /// Represents a nullable <see cref="DateOnly"/> object which holds date information
    /// if successfully parsed; otherwise, null.
    /// </summary>
    public DateOnly? Date { get; set; }

    /// <summary>
    /// Gets or sets a dictionary of pattern matches extracted during the file path parsing process.
    /// Each key represents a named group or pattern identifier, and its corresponding value
    /// holds the matched content from the parsed path.
    /// </summary>
    public Dictionary<string, string> Matches { get; set; } = new ();
}