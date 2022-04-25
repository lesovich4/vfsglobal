using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VfsGlobalHtmLogger.Console
{
    public class HtmlArchiverHostedService : TimedHostedService
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<HtmlArchiverConfiguration> _configuration;
        private readonly HtmlArchiverCommand _htmlArchiverCommand;

        public HtmlArchiverHostedService(
            ILogger<HtmlArchiverHostedService> logger,
            IOptionsMonitor<HtmlArchiverConfiguration> configuration,
            HtmlArchiverCommand htmlArchiverCommand)
            : base(logger)
        {
            _logger = logger;
            _configuration = configuration;
            _htmlArchiverCommand = htmlArchiverCommand;
        }

        protected override TimeSpan TimerPeriod => TimeSpan.FromMinutes(_configuration.CurrentValue.PullIntervalMinutes);

        protected override async Task RunJobAsync(CancellationToken stoppingToken)
        {
            var configuration = _configuration.CurrentValue;
            _logger.LogInformation("Log options {0}", configuration);

            var archiveCapacity = TimeSpan.FromHours(configuration.ArchiveCapacityHours);
            await Task.WhenAll(configuration.Urls.Select(url => _htmlArchiverCommand.Archive(new HtmlArchiverArgs(url, archiveCapacity), stoppingToken)));

        }
    }
}
