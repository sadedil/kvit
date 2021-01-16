using System.IO;
using System.Text;

namespace Kvit.IntegrationTests.TestHelpers
{
    internal static class FileHelper
    {
        public static readonly Encoding TestEncoding = Encoding.UTF8;

        public static void WriteAllText(string filePath, string contents)
        {
            var fileInfo = new FileInfo(filePath);
            Directory.CreateDirectory(fileInfo.Directory.FullName);
            File.WriteAllText(fileInfo.FullName, contents, TestEncoding);
        }
    }
}