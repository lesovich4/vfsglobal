using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.OffScreen;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VfsGlobalHtmLogger.Console.Resources;

namespace VfsGlobalHtmLogger.Console
{
    public class HtmlArchiverCommand
    {
        private readonly ILogger _logger;
        private readonly ChromiumWebBrowser _browser;

        public HtmlArchiverCommand(ILogger<HtmlArchiverCommand> logger, ChromiumWebBrowser browser)
        {
            _logger = logger;
            _browser = browser;
        }

        public async Task Archive(HtmlArchiverArgs args, CancellationToken cancellationToken)
        {
            await _browser.GetCookieManager().DeleteCookiesAsync();
            _logger.LogInformation("Start loading page {0}", args.ArchiveUrl);
            var response = await _browser.LoadUrlAsync(args.ArchiveUrl);
            _logger.LogInformation("Finish loading. Status code {0}", response.HttpStatusCode);
            _browser.Stop();
            var now = DateTime.Now;
            await WriteDocument(now, cancellationToken);
            ClearFolder(now - args.ArchiveCapacityTimeSpan);
        }

        private void ClearFolder(DateTime time)
        {
            var folder = EnsureFolder(time, _browser.Address);

            foreach (var path in Directory.GetFiles(folder))
            {
                var created = File.GetCreationTime(path);
                if (created < time)
                {
                    _logger.LogInformation("The file is outdated. Removing {0}", Path.GetFileName(path));
                    File.Delete(path);
                    _logger.LogInformation("The file is removed");
                }
            }
        }

        private async Task WriteDocument(DateTime time, CancellationToken cancellationToken)
        {
            var folder = EnsureFolder(time, _browser.Address);

            var name = await GetName();
            var fileName = GetFileName(time, $"{name}.html");
            var filePath = Path.Combine(folder, fileName);
            using var file = File.OpenWrite(filePath);
            using var xmlWriter = XmlWriter.Create(file, new XmlWriterSettings { Async = true });

            xmlWriter.WriteStartElement("html");
            await WriteHead(xmlWriter, cancellationToken);
            await WriteBody(xmlWriter, cancellationToken);
            xmlWriter.WriteEndElement();

        }

        private string EnsureFolder(DateTime time, string address)
        {
            var folder = GetFolder(time, address);
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        private async Task<string> GetDocumentTitle()
        {

            var script = @"(function () {
                var lastUpdatedEl = document.getElementById(
                        'last-updated'
                      );
                if(!lastUpdatedEl) {
                    return document.title;
                }
                var waitTimeEl = document.getElementsByTagName('h2')[0];
                var title = lastUpdatedEl.innerText + ' ' + waitTimeEl.innerText.replace('Your estimated wait time is', '');
                return title;
            })();";
            var scriptResponse = await _browser.EvaluateScriptAsync(script);
            return scriptResponse.Result.ToString();
        }

        private async Task<string> GetName()
        {

            var script = @"(function () {
                var lastUpdatedEl = document.getElementById(
                        'last-updated'
                      );
                if(!lastUpdatedEl) {
                    return 'archive';
                }
                var waitTimeEl = document.getElementsByTagName('h2')[0];
                var fileName = waitTimeEl.innerText.replace('Your estimated wait time is', '');
                return fileName
            })();";
            var scriptResponse = await _browser.EvaluateScriptAsync(script);
            return scriptResponse.Result.ToString();
        }

        private async Task WriteHead(XmlWriter xmlWriter, CancellationToken cancellationToken)
        {
            var title = await GetDocumentTitle();

            var header = new XElement("head",
                new XElement("title", title),
                new XElement("style", Styles.Acrhive),
                new XElement("script", Scripts.Acrhive)
            );
            await header.WriteToAsync(xmlWriter, cancellationToken);
        }
        private async Task WriteBody(XmlWriter xmlWriter, CancellationToken cancellationToken)
        {
            xmlWriter.WriteStartElement("body");
            await WriteCookies(xmlWriter, cancellationToken);
            await WriteScreenshot(xmlWriter, cancellationToken);
            xmlWriter.WriteEndElement();
        }

        private async Task WriteCookies(XmlWriter xmlWriter, CancellationToken cancellationToken)
        {
            using var cookieManager = _browser.GetCookieManager();
            var cookies = await cookieManager.VisitAllCookiesAsync();

            _logger.LogInformation("Cookies ready. Saving");

            var element =
                new XElement("div",
                    new XElement("h1", "Cookies"),
                    new XElement("table",
                        new XElement("thead",
                            new XElement("tr",
                                    new XElement("th", "Name"),
                                    new XElement("th", "Value"),
                                    new XElement("th", new XAttribute("style", "width: 200px;"), "Expires"),
                                    new XElement("th", new XAttribute("style", "width: 200px;"), "Domain"),
                                    new XElement("th", new XAttribute("style", "width: 200px;"), "Path"),
                                    new XElement("th", new XAttribute("style", "width: 60px;"), "Action")
                            )
                        ),
                        new XElement("tbody",
                            cookies.Select(cookie =>
                                new XElement("tr",
                                    new XElement("td", new XAttribute("title", cookie.Name ?? String.Empty), new XAttribute("class", "name"), cookie.Name),
                                    new XElement("td", new XAttribute("title", cookie.Value ?? String.Empty), new XAttribute("class", "value"), cookie.Value),
                                    new XElement("td", new XAttribute("title", $"{cookie.Expires:o}"), new XAttribute("class", "expires"), cookie.Expires),
                                    new XElement("td", new XAttribute("title", cookie.Domain ?? String.Empty), new XAttribute("class", "domain"), cookie.Domain),
                                    new XElement("td", new XAttribute("title", cookie.Path ?? String.Empty), new XAttribute("class", "path"), cookie.Path),
                                    new XElement("td",
                                        new XElement("button", new XAttribute("onclick", "clickCopy(this)"), "Copy")
                                    )
                                )
                            )
                        )
                    )
                );

            await element.WriteToAsync(xmlWriter, cancellationToken);

            _logger.LogInformation("Cookies saved.");
        }

        private async Task WriteScreenshot(XmlWriter xmlWriter, CancellationToken cancellationToken)
        {
            var image = await _browser.CaptureScreenshotAsync(CaptureScreenshotFormat.Png);


            _logger.LogInformation("Screenshot ready. Saving");

            var element =
                new XElement("div",
                    new XElement("h1", "Screenshot"),
                    new XElement("img",
                        new XAttribute("src", "data:image/png;base64," + Convert.ToBase64String(image))
                    )
                );

            await element.WriteToAsync(xmlWriter, cancellationToken);

            _logger.LogInformation("Screenshot saved.");

        }


        private static readonly Regex InvalidCharsRegex =
            new(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))));
        private string GetFolder(DateTime dateTime, string address)
        {
            var uriPath = new UriBuilder(address).Path;
            var fileName = Path.GetFileName(uriPath);
            var folderName = InvalidCharsRegex.Replace(fileName, "_");
            return Path.GetFullPath($"./{folderName}");
        }

        private string GetFileName(DateTime dateTime, string name) =>
            $"{dateTime:yyyy-MM-ddTHH_mm_ss}_{name}";
    }
}
