// <copyright file="PathParserPriority.cs" company="SharpPathParser contributors">
// Copyright (c) SharpPathParser contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SharpPathParser.Enum;

/// <summary>
/// Whether to match the whole path or just the filename.
/// </summary>
public enum PathParserPriority
{
    /// <summary>
    /// Only parses the file name
    /// </summary>
    Filename,

    /// <summary>
    /// Parses the whole file path
    /// </summary>
    Path,
}