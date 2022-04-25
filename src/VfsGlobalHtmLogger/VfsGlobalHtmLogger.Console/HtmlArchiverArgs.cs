using System;

namespace VfsGlobalHtmLogger.Console
{
    public record HtmlArchiverArgs(string ArchiveUrl, TimeSpan ArchiveCapacityTimeSpan);
}
