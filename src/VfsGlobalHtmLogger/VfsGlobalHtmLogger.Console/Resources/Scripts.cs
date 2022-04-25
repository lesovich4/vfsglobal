using System.IO;

namespace VfsGlobalHtmLogger.Console.Resources
{
    public static class Scripts
    {
        public static readonly string Acrhive = new StreamReader(typeof(Styles).Assembly.GetManifestResourceStream(typeof(Styles), "archive.js")).ReadToEnd();
    }
}
