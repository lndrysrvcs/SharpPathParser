// <copyright file="PatternMatcherTest.cs" company="SharpPathParser contributors">
// Copyright (c) SharpPathParser contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SharpPathParser.Tests;

using System;
using System.Collections.Generic;
using NUnit.Framework;


[TestFixture]
public class PatternMatcherTest
{
    [Test]
    public void MatchGroups_ShouldMatchCorrectly_WhenValidGroupsAndBlocksProvided()
    {
        // Arrange
        var groups = new Dictionary<string, IEnumerable<string>>
        {
            { "group1", new List<string> { "Apple", "Banana", "Cherry" } },
            { "group2", new List<string> { "Dog", "Elephant" } },
        };
        var blocks = new[] { "Banana", "Dog", "Giraffe", "Banana" };

        // Act
        var result = PatternMatcher.MatchGroups(groups, blocks);

        // Assert
        Assert.That(2, Is.EqualTo(result.Count));
        Assert.That("Banana", Is.EqualTo(result["group1"]));
        Assert.That("Dog", Is.EqualTo(result["group2"]));
    }

    [Test]
    public void MatchGroups_ShouldReturnEmpty_WhenNoMatchingBlocks()
    {
        // Arrange
        var groups = new Dictionary<string, IEnumerable<string>>
        {
            { "group1", new List<string> { "Apple", "Banana" } },
            { "group2", new List<string> { "Car", "Dog" } },
        };
        var blocks = new[] { "Tree", "Rock", "River" };

        // Act
        var result = PatternMatcher.MatchGroups(groups, blocks);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MatchGroups_ShouldHandleNullGroups()
    {
        // Act
        var result = PatternMatcher.MatchGroups(null, new[] { "Anything" });

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MatchTime_ShouldReturnTime_WhenValidTimeExists()
    {
        // Arrange
        var blocks = new[] { "12:34", "SomeText" };

        // Act
        var result = PatternMatcher.MatchTime(blocks);

        // Assert
        Assert.That(result, Is.Not.Null);;
        Assert.That(result.Value.Hour, Is.EqualTo(12));
        Assert.That(result.Value.Minute, Is.EqualTo(34));
    }

    [Test]
    public void MatchTime_ShouldReturnNull_WhenNoValidTimeExists()
    {
        // Arrange
        var blocks = new[] { "Invalid", "Text" };

        // Act
        var result = PatternMatcher.MatchTime(blocks);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MatchTime_ShouldPerformSlidingWindowSearch_WhenDeepSearchEnabled()
    {
        // Arrange
        var blocks = new[] { "12", "34", "AM", "Text" };

        // Act
        var result = PatternMatcher.MatchTime(blocks, true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(new TimeOnly(0, 34)));
    }

    [Test]
    public void MatchTime_ShouldReturnNull_WhenDeepSearchFails()
    {
        // Arrange
        var blocks = new[] { "Invalid", "Fragments" };

        // Act
        var result = PatternMatcher.MatchTime(blocks, true);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MatchDate_ShouldReturnDate_WhenValidDateExists()
    {
        // Arrange
        var blocks = new[] { "2025-06-26", "AnotherBlock" };

        // Act
        var result = PatternMatcher.MatchDate(blocks);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.Year, Is.EqualTo(2025));
        Assert.That(result.Value.Month, Is.EqualTo(6));
        Assert.That(result.Value.Day, Is.EqualTo(26));
    }

    [Test]
    public void MatchDate_ShouldReturnNull_WhenNoValidDateExists()
    {
        // Arrange
        var blocks = new[] { "InvalidDate", "Text" };

        // Act
        var result = PatternMatcher.MatchDate(blocks);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MatchDate_ShouldPerformSlidingWindowSearch_WhenDeepSearchEnabled()
    {
        // Arrange
        var blocks = new[] { "2025", "06", "26" };

        // Act
        var result = PatternMatcher.MatchDate(blocks, true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.Year, Is.EqualTo(2025));
        Assert.That(result.Value.Month, Is.EqualTo(6));
        Assert.That(result.Value.Day, Is.EqualTo(26));
    }

    [Test]
    public void MatchDate_ShouldReturnNull_WhenDeepSearchFails()
    {
        // Arrange
        var blocks = new[] { "Invalid", "DateFragments" };

        // Act
        var result = PatternMatcher.MatchDate(blocks, true);

        // Assert
        Assert.That(result, Is.Null);
    }
}