using CefSharp;
using CefSharp.Handler;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VfsGlobalHtmLogger.Console.Handlers
{

    public class LoggingRequestHandler : RequestHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public LoggingRequestHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return _serviceProvider.GetService<LoggingResourceRequestHandler>();
        }
    }
}
