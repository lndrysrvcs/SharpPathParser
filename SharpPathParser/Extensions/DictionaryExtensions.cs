// <copyright file="DictionaryExtensions.cs" company="SharpPathParser contributors">
// Copyright (c) SharpPathParser contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace SharpPathParser.Extensions;

/// <summary>
/// Provides extension methods for dictionaries.
/// </summary>
internal static class DictionaryExtensions
{
    /// <summary>
    /// Adds all key-value pairs from the source dictionary to the destination dictionary.
    /// </summary>
    /// <param name="destDictionary">The dictionary to which the key-value pairs will be added.</param>
    /// <param name="sourceDictionary">The dictionary containing the key-value pairs to add.</param>
    /// <typeparam name="TKey">The type of the keys in the dictionaries.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionaries.</typeparam>
    internal static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> destDictionary, IDictionary<TKey, TValue> sourceDictionary)
    {
        foreach (KeyValuePair<TKey, TValue> keyValuePair in sourceDictionary)
        {
            destDictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}