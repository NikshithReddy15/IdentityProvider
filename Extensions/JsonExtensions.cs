using System.Text.Json;

namespace Extensions;

public static class JsonExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
    
    public static T? FromJson<T>(this string obj)
    {
        if (string.IsNullOrEmpty(obj))
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<T>(obj);
    }
}