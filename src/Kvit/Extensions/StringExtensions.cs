namespace Kvit.Extensions
{
    /// <summary>
    /// Helper methods for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Maybe the name was misleading but unix paths are valid for Unix and Windows environments
        /// But Windows paths are not
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToUnixPath(this string source)
        {
            return source.Replace('\\', '/');
        }

        public static string Colorize(this string value, string color)
        {
            return $"[{color}]{value}[/]";
        }
    }
}