/*
 * This file is largely based upon
 * https://github.com/pardeike/Harmony/blob/11f3a1de4c512f9da39fed8a15fc1e8f5fa397a3/Harmony/Tools/AccessTools.cs
 * Copyright (c) 2017 Andreas Pardeike
 * Andreas Pardeike licenses the referenced file to Joe Clack under the MIT license.
 *
 * Copyright (c) 2024 Joe Clack
 * Joe Clack licenses this file to you under the GPL-3.0-or-later license.
 */

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace StardewMouseTweaks;

public static class AccessToolsPolyfill
{
    /// <summary>Gets the reflection information for a directly declared indexer property</summary>
    /// <param name="type">The class/type where the indexer property is declared</param>
    /// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
    /// <returns>An indexer property or null when type is null or when it cannot be found</returns>
    ///
    public static PropertyInfo DeclaredIndexer(Type type, Type[]? parameters = null)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            // Can find multiple indexers without specified parameters, but only one with specified ones
            var indexer = parameters is null ?
                type.GetProperties(AccessTools.allDeclared).SingleOrDefault(property => property.GetIndexParameters().Length > 0)
                : type.GetProperties(AccessTools.allDeclared).FirstOrDefault(property => property.GetIndexParameters().Select(param => param.ParameterType).SequenceEqual(parameters));

            if (indexer is null) throw new InvalidOperationException($"AccessTools.DeclaredIndexer: Could not find indexer for type {type} and parameters {parameters?.Description()}");

            return indexer;
        }
        catch (InvalidOperationException ex)
        {
            throw new AmbiguousMatchException("Multiple possible indexers were found.", ex);
        }
    }

    /// <summary>Gets the reflection information for the getter method of a directly declared indexer property</summary>
    /// <param name="type">The class/type where the indexer property is declared</param>
    /// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
    /// <returns>A method or null when type is null or when indexer property cannot be found</returns>
    ///
    public static MethodInfo? DeclaredIndexerGetter(Type type, Type[]? parameters = null) => DeclaredIndexer(type, parameters).GetGetMethod(true);

    /// <summary>Gets the reflection information for the setter method of a directly declared indexer property</summary>
    /// <param name="type">The class/type where the indexer property is declared</param>
    /// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
    /// <returns>A method or null when type is null or when indexer property cannot be found</returns>
    ///
    public static MethodInfo? DeclaredIndexerSetter(Type type, Type[]? parameters) => DeclaredIndexer(type, parameters).GetSetMethod(true);
}
