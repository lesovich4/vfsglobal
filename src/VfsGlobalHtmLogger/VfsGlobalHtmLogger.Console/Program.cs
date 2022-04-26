using CefSharp;
using CefSharp.BrowserSubprocess;
using CefSharp.OffScreen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using VfsGlobalHtmLogger.Console;
using VfsGlobalHtmLogger.Console.Handlers;

class Program
{
    public static int Main(string[] args)
    {
        Cef.EnableHighDPISupport();

        var exitCode = SelfHost.Main(args);

        if (exitCode >= 0)
        {
            return exitCode;
        }

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<LoggingRequestHandler>();
                services.AddTransient<LoggingResourceRequestHandler>();
                services.AddTransient(sp =>
                {
                    var browser = new ChromiumWebBrowser();
                    browser.RequestHandler = sp.GetRequiredService<LoggingRequestHandler>();
                    return browser;
                });
                services.AddTransient<HtmlArchiverCommand>();
                services.Configure<HtmlArchiverConfiguration>(context.Configuration.GetSection("HtmlArchiver"));
                services.AddHostedService<HtmlArchiverHostedService>();
            })
            .Build();
        

        var settings = new CefSettings()
        {
            CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            BrowserSubprocessPath = Process.GetCurrentProcess().MainModule.FileName
        };
        Cef.Initialize(settings, performDependencyCheck: false);

        host.Run();
        Cef.Shutdown();

        return 0;
    }

}