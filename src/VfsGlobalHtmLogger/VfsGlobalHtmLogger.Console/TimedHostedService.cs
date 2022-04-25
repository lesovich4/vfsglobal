using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VfsGlobalHtmLogger.Console
{
    public abstract class TimedHostedService : IHostedService, IDisposable
    {
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly ILogger _logger;
        private Timer? _timer;
        private Task? _executingTask;

        public TimedHostedService(ILogger logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timer = new Timer(ExecuteTask, null, 0, Timeout.Infinite);

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);

            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
            _timer?.Dispose();
        }

        protected abstract Task RunJobAsync(CancellationToken stoppingToken);

        protected abstract TimeSpan TimerPeriod { get; }

        private void ExecuteTask(object? state)
        {
            _executingTask = ExecuteTaskAsync(_stoppingCts.Token);
        }

        private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
        {
            try
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                await RunJobAsync(stoppingToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error");
            }
            finally
            {
                _timer?.Change(TimerPeriod, Timeout.InfiniteTimeSpan);
            }
        }
    }
}
