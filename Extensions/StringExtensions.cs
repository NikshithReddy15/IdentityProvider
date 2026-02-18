namespace Extensions;

public static class StringExtensions
{
    public static Guid? ToGuid(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        
        return Guid.Parse(str);
    }
}