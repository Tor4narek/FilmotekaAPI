using System.Security.Cryptography;
using System.Text;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using PuppeteerSharp;

namespace FilmotekaAPI.Services;

// Singleton: owns one browser instance shared across all requests.
public sealed class VideoExtractionService : IVideoExtractionService, IAsyncDisposable
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<VideoExtractionService> _logger;
    private readonly IConfiguration _config;
    private IBrowser? _browser;
    private readonly SemaphoreSlim _browserLock = new(1, 1);

    private static readonly HashSet<string> AdDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "doubleclick.net", "googlesyndication.com", "adnxs.com",
        "moatads.com", "adsrvr.org", "casalemedia.com", "rubiconproject.com"
    };

    public VideoExtractionService(
        IDistributedCache cache,
        ILogger<VideoExtractionService> logger,
        IConfiguration config)
    {
        _cache = cache;
        _logger = logger;
        _config = config;
    }

    public async Task<string?> ExtractAsync(string iframeUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(iframeUrl)) return null;

        var cacheKey = $"video:{ComputeSha256(iframeUrl)}";

        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null) return cached;

        var browser = await EnsureBrowserAsync(ct);
        IPage? page = null;

        try
        {
            page = await browser.NewPageAsync();

            await page.SetUserAgentAsync(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                ["Referer"] = new Uri(iframeUrl).GetLeftPart(UriPartial.Authority)
            });

            string? foundUrl = null;
            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

            page.Request += (_, e) =>
            {
                var url = e.Request.Url;
                if (IsVideoUrl(url) && !IsAdUrl(url))
                {
                    if (foundUrl is null || url.Contains(".m3u8", StringComparison.OrdinalIgnoreCase))
                    {
                        foundUrl = url;
                        tcs.TrySetResult(url);
                    }
                }
            };

            var navTimeout = _config.GetValue("Puppeteer:NavigationTimeoutMs", 30_000);
            var gracePeriod = _config.GetValue("Puppeteer:GracePeriodMs", 5_000);

            using var navCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            navCts.CancelAfter(navTimeout + gracePeriod);

            try
            {
                await page.GoToAsync(iframeUrl, new NavigationOptions
                {
                    Timeout = navTimeout,
                    WaitUntil = [WaitUntilNavigation.Networkidle2]
                });
            }
            catch (NavigationException)
            {
                // Timeout on networkidle2 is expected for video pages — proceed to grace period.
            }

            // Wait for video URL interception or grace period, whichever comes first.
            var delayTask = Task.Delay(gracePeriod, navCts.Token);
            await Task.WhenAny(tcs.Task, delayTask);

            var result = foundUrl ?? (tcs.Task.IsCompletedSuccessfully ? tcs.Task.Result : null);

            if (result is not null)
            {
                await _cache.SetStringAsync(cacheKey, result,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40) },
                    CancellationToken.None);

                _logger.LogInformation("Extracted video URL for {IframeUrl}: {VideoUrl}", iframeUrl, result);
            }
            else
            {
                _logger.LogWarning("No video URL found for {IframeUrl}", iframeUrl);
            }

            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting video from {IframeUrl}", iframeUrl);
            // Browser may be in a bad state — reset it.
            await ResetBrowserAsync();
            return null;
        }
        finally
        {
            if (page is not null)
            {
                try { await page.CloseAsync(); } catch { /* best-effort */ }
            }
        }
    }

    private async Task<IBrowser> EnsureBrowserAsync(CancellationToken ct)
    {
        if (_browser is { IsConnected: true })
            return _browser;

        await _browserLock.WaitAsync(ct);
        try
        {
            if (_browser is { IsConnected: true })
                return _browser;

            _browser?.Dispose();
            _browser = await LaunchBrowserAsync();
            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }

    private async Task<IBrowser> LaunchBrowserAsync()
    {
        var executablePath = _config["Puppeteer:ExecutablePath"];

        // Download Chromium if no custom path is set.
        if (string.IsNullOrEmpty(executablePath))
        {
            _logger.LogInformation("Downloading Chromium for Puppeteer...");
            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync();
        }

        var launchOptions = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = string.IsNullOrEmpty(executablePath) ? null : executablePath,
            Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage", "--disable-gpu"]
        };

        _logger.LogInformation("Launching Chromium browser...");
        return await Puppeteer.LaunchAsync(launchOptions);
    }

    private async Task ResetBrowserAsync()
    {
        await _browserLock.WaitAsync();
        try
        {
            if (_browser is not null)
            {
                try { await _browser.CloseAsync(); } catch { /* ignore */ }
                _browser.Dispose();
                _browser = null;
            }
        }
        finally
        {
            _browserLock.Release();
        }
    }

    private static bool IsVideoUrl(string url)
    {
        var lower = url.ToLower();
        return lower.Contains(".m3u8") || lower.Contains(".mp4") || lower.Contains("manifest");
    }

    private static bool IsAdUrl(string url)
    {
        try
        {
            var host = new Uri(url).Host;
            return AdDomains.Any(d => host.EndsWith(d, StringComparison.OrdinalIgnoreCase));
        }
        catch { return false; }
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            try { await _browser.CloseAsync(); } catch { /* ignore */ }
            _browser.Dispose();
        }
        _browserLock.Dispose();
    }
}
