using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metatool.Plugin;

public static class PropertyExtensions
{
    /// <summary>
    /// get all properties from obj recursively
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="genericTypeDefinition"></param>
    /// <param name="maxDepth"></param>
    /// <returns></returns>
    public static List<(string Path, PropertyInfo Property, object Parent)> GetPropertiesOfType(this object obj, Type genericTypeDefinition, int maxDepth = 5)
    {
        var results = new List<(string, PropertyInfo, object)>();
        var visited = new HashSet<object>();

        if (obj == null)
            return results;

        TraverseProperties(obj, genericTypeDefinition, "", results, visited, 0, maxDepth);
        return results;
    }

    private static void TraverseProperties(
        object obj,
        Type targetType,
        string path,
        List<(string, PropertyInfo, object)> results,
        HashSet<object> visited,
        int currentDepth,
        int maxDepth)
    {
        if (obj == null || currentDepth >= maxDepth)
            return;

        // Prevent infinite loops from circular references
        var type = obj.GetType();
        if (type.IsValueType || type.IsPrimitive || type == typeof(string) || visited.Contains(obj))
            return;

        visited.Add(obj);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            // Skip indexed properties
            if (prop.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var currentPath = string.IsNullOrEmpty(path)
                    ? prop.Name
                    : $"{path}.{prop.Name}";

                // Check if this property is of target type
                if (targetType.IsAssignableFrom(prop.PropertyType) || prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == targetType)
                {
                    // Store the PropertyInfo and parent object so we can modify it later
                    results.Add((currentPath, prop, obj));
                    continue;
                }

                var value = prop.GetValue(obj);
                TraverseProperties(value, targetType, currentPath, results, visited, currentDepth + 1, maxDepth);

            }
            catch (Exception)
            {
                // Skip properties that throw exceptions when accessed
                continue;
            }
        }
    }
}