using System;

namespace VfsGlobalHtmLogger.Console
{
    public record HtmlArchiverConfiguration
    {
        public string[] Urls { get; set; } = Array.Empty<string>();
        public int PullIntervalMinutes { get; set; }
        public int ArchiveCapacityHours { get; set; }

    }
}
