using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;


namespace Metatool.Service.Keyboard;
/// <summary>
/// the data of T is the context, it is dynamic
/// </summary>
/// <typeparam name="T"></typeparam>
public class ContextHotkey<T>
{
    public Dictionary<string, ContextHotkey<T>>? Children { get; set; }
    public string Description { get; set; }
    public T Context { get; set; }

    public void Visit(Action<string, ContextHotkey<T>> sequenceKeyHandler, string path = "")
    {
        sequenceKeyHandler(path, this);
        if (Children == null) return;//tip

        var parentPath = $"{path},".TrimStart(',');

        foreach (var child in Children)
        {
            child.Value.Visit(sequenceKeyHandler, $"{parentPath}{child.Key}");
        }
    }
}

public class ContextHotkeyCommandConfig(IConfiguration configuration)
{
    /// <summary>
    /// generate value from config in json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sectionPathStartFromCurrentTool"></param>
    /// <returns></returns>
    public ContextHotkey<T> Generate<T>(string sectionPathStartFromCurrentTool)
    {
        var section = configuration.GetSection(sectionPathStartFromCurrentTool);
        return Parse<T>(section);
    }
    /// <summary>
    /// call ContextHotkey<T> Generate<T>(string sectionPathStartFromCurrentTool) in no generic way
    /// return ContextHotkey<T>
    /// </summary>
    public object Generate(Type contextType, string sectionPathStartFromCurrentTool)
    {
        var genMethod = typeof(ContextHotkeyCommandConfig).GetMethod(nameof(ContextHotkeyCommandConfig.Generate),
            [typeof(string)]).MakeGenericMethod(contextType);
        return genMethod.Invoke(this, [sectionPathStartFromCurrentTool]);
    }

    public static ContextHotkey<T> Parse<T>(IConfigurationSection section, ContextHotkey<T> sequenceHotKey = null)
    {
        sequenceHotKey ??= new ContextHotkey<T>();
        foreach (var child in section.GetChildren())
        {
            if (child.Key == "Description")
            {
                sequenceHotKey.Description =
                    child.Value; // when is a string we can use the Value property, for an obj it's null
            }
            else
            {
                var childValueSec = section.GetSection(child.Key); // get the value or the children section
                var res = TryGet<T>(childValueSec, out T obj);
                if (res)
                {
                    var subSequenceHotKey = new ContextHotkey<T>
                    {
                        Context = obj// (T)(object)child.Value
                    };
                    subSequenceHotKey.Description ??= obj.ToString();
                    sequenceHotKey.Children ??= new();
                    sequenceHotKey.Children.Add(child.Key, subSequenceHotKey);
                }
                else
                {
                    var subSequenceHotKey = Parse<T>(childValueSec);
                    sequenceHotKey.Children ??= new();
                    sequenceHotKey.Children.Add(child.Key, subSequenceHotKey);
                }
            }

        }

        return sequenceHotKey;
    }
    static bool TryGet<T>(IConfigurationSection section, out T result)
    {
        if (!section.Exists())
        {
            result = default;
            return false;
        }

        try
        {
            result = section.Get<T>();
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }


}

