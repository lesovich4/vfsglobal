using CefSharp;
using CefSharp.BrowserSubprocess;
using CefSharp.OffScreen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using VfsGlobalHtmLogger.Console;

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

        var settings = new CefSettings()
        {
            CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            BrowserSubprocessPath = Process.GetCurrentProcess().MainModule.FileName
        };
        Cef.Initialize(settings, performDependencyCheck: false);

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<ChromiumWebBrowser>();
                services.AddTransient<HtmlArchiverCommand>();
                services.Configure<HtmlArchiverConfiguration>(context.Configuration.GetSection("HtmlArchiver"));
                services.AddHostedService<HtmlArchiverHostedService>();
            })
            .Build();
        host.Run();
        Cef.Shutdown();

        return 0;
    }

}