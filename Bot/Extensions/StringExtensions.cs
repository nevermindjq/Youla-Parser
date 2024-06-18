namespace Bot.Extensions;

public static class StringExtensions {
    public static string NetscapeValue(this string str) => str[(str.LastIndexOf('\t') + 1)..];
}