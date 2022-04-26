namespace VfsGlobalHtmLogger.Console
{
    public record HtmlArchiverConfiguration
    {
        public string Url { get; set; }
        public int PullIntervalMinutes { get; set; }
        public int ArchiveCapacityHours { get; set; }

    }
}
