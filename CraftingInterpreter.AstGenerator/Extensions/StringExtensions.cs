namespace CraftingInterpreter.AstGenerator.Extensions;

public static class StringExtensions
{
    public static string Capitalize(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        s = s.Trim();

        if (s.Length == 1)
            return s.ToUpper();
        
        return char.ToUpper(s[0]) + s[1..];
    }
}