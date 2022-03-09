using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace tone.Common.Extensions.Object;

public static class ObjectExtensions
{
    public static object? GetPropertyValue(this object? obj, string name) {
        var parts = name.Split('.');
        foreach(var part in parts) {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();
            var info = type.GetProperty(part);
            if (info == null)
            {
                return null;
            }
            obj = info.GetValue(obj, null);
        }

        return obj;
    }
    

    public static T? GetPropertyValue<T>(this object? obj, string name) {
        var returnValue = GetPropertyValue(obj, name);
        if (returnValue == null)
        {
            return default;
        }
        return (T) returnValue;
    }
    
    public static IEnumerable<PropertyInfo> GetProperties<T>(this T obj)
    {
        return typeof(T).GetProperties();
    }

    public static void SetPropertyValue<T>(this object? obj, string name, T value)
    {
        var parts = name.Split('.');
        var lastIndex = parts.Length - 1;
        for (var i=lastIndex;i>=1;i--) {
            if (obj == null)
            {
                return;
            }
            var type = obj.GetType();
            var info = type.GetProperty(parts[i]);
            if (info == null)
            {
                return;
            }

            if (i == 1)
            {
                info.SetValue(obj, value);
                return;
            }
            obj = info.GetValue(obj, null);
        }
    }
    
}