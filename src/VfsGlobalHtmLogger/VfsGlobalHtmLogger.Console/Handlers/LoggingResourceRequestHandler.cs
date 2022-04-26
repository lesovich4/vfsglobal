using CefSharp;
using CefSharp.Handler;
using Microsoft.Extensions.Logging;

namespace VfsGlobalHtmLogger.Console.Handlers
{
    public class LoggingResourceRequestHandler : ResourceRequestHandler
    {
        private readonly ILogger<LoggingResourceRequestHandler> _logger;

        public LoggingResourceRequestHandler(ILogger<LoggingResourceRequestHandler> logger)
        {
            _logger = logger;
        }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            _logger.LogDebug("Loading resource {0} {1}", request.Method, request.Url);
            return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);
        }
    }
}
