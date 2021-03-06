using Spectre.Console;

namespace Kvit.Extensions
{
    /// <summary>
    /// Helper methods for formatting console
    /// </summary>
    public static class FormattingExtensions
    {
        private static string ToFormat(this string value, string color, bool isBold = false)
        {
            var boldIdentifier = isBold ? "bold " : string.Empty;
            return $"[{boldIdentifier}{color}]{value.EscapeMarkup()}[/]";
        }

        public static string ToYellow(this string value, bool isBold = false) => value.ToFormat("gold1", isBold);
        public static string ToGreen(this string value, bool isBold = false) => value.ToFormat("green", isBold);
        public static string ToRed(this string value, bool isBold = false) => value.ToFormat("red", isBold);
        public static string ToSilver(this string value, bool isBold = false) => value.ToFormat("grey74", isBold);
        public static string ToGrey(this string value, bool isBold = false) => value.ToFormat("grey", isBold);
    }
}