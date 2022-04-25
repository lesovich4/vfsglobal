using System.IO;

namespace VfsGlobalHtmLogger.Console.Resources
{
    public static class Styles
    {
        public static readonly string Acrhive = new StreamReader(typeof(Styles).Assembly.GetManifestResourceStream(typeof(Styles), "archive.css")).ReadToEnd();
    }
}
