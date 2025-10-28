using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru.Mobile
{
    public static class FileDownloader
    {
        private static readonly HttpClient httpClient = new HttpClient();
        /// <summary>
        /// 下载文件（流式），显示进度并在出错时重试指定次数。
        /// progress.Report(value): value in [0.0,1.0]; 当内容长度未知时报告 -1.0。
        /// </summary>
        /// <summary>
        /// C# 7.3 兼容：流式下载、显示进度并在出错时重试指定次数。
        /// progress.Report(value): value in [0.0,1.0]; 当内容长度未知时报告 -1.0。
        /// </summary>
        //public static async Task<bool> DownloadFileAsync(
        //    string url,
        //    string destPath,
        //    int maxAttempts = 3,
        //    IProgress<double> progress = null,
        //    TimeSpan? baseDelay = null,
        //    HttpClient httpClient = null,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //{

            
        //    if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        //    if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
        //    baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
        //    var rng = new Random();

        //    bool disposeClient = false;
        //    if (httpClient == null)
        //    {
        //        httpClient = new HttpClient();
        //        disposeClient = true;
        //    }

        //    var tmpPath = destPath + ".partial";

        //    try
        //    {
        //        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        //        {
        //            try
        //            {
        //                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
        //                using (var resp = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
        //                {
        //                    resp.EnsureSuccessStatusCode();

        //                    var contentLength = resp.Content.Headers.ContentLength;

        //                    Directory.CreateDirectory(Path.GetDirectoryName(destPath) ?? "");

        //                    using (var responseStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
        //                    using (var fileStream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        //                    {
        //                        var buffer = new byte[81920];
        //                        long totalRead = 0;
        //                        int read;
        //                        if (!contentLength.HasValue)
        //                        {
        //                            progress?.Report(-1.0);
        //                        }
        //                        while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        //                        {
        //                            await fileStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
        //                            totalRead += read;
        //                            if (contentLength.HasValue)
        //                            {
        //                                progress?.Report(Math.Min(1.0, (double)totalRead / contentLength.Value));
        //                            }
        //                        }
        //                    }

        //                    // Replace final file
        //                    if (File.Exists(destPath)) File.Delete(destPath);
        //                    File.Move(tmpPath, destPath);
        //                    progress?.Report(1.0);
        //                    return true;
        //                }
        //            }
        //            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        //            {
        //                throw;
        //            }
        //            catch (Exception) when (attempt < maxAttempts)
        //            {
        //                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }

        //                var delayMs = baseDelay.Value.TotalMilliseconds * Math.Pow(2, attempt - 1);
        //                var jitterMs = rng.Next(0, 300);
        //                var delay = TimeSpan.FromMilliseconds(delayMs + jitterMs);
        //                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        //            }
        //            // 最后一轮异常会抛出
        //        }
        //        return false;
        //    }
        //    finally
        //    {
        //        try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
        //        if (disposeClient)
        //        {
        //            try { httpClient.Dispose(); }
        //            catch
        //            {
                        
        //            }
        //        }
        //    }
        //}

        public static async Task<bool> DownloadFileAsync(
        string url,
        string destPath,
        int maxAttempts = 3,
        IProgress<double> progress = null,
        TimeSpan? baseDelay = null,
        HttpClient httpClient = null,
        TimeSpan? timeoutPerAttempt = null,
        CancellationToken cancellationToken = default(CancellationToken))
        {
            if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
            var rng = new Random();
            if (timeoutPerAttempt == null)
                timeoutPerAttempt = TimeSpan.FromMinutes(3);

            bool disposeClient = false;
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                disposeClient = true;
            }

            var tmpPath = destPath + ".partial";

            try
            {
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    // Create CTS for this attempt: link external cancellation and optional timeout
                    using (var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        if (timeoutPerAttempt.HasValue)
                        {
                            try { attemptCts.CancelAfter(timeoutPerAttempt.Value); } catch { }
                        }

                        try
                        {
                            using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                            using (var resp = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, attemptCts.Token).ConfigureAwait(false))
                            {
                                if (!resp.IsSuccessStatusCode)
                                {
                                    // treat non-success as transient
                                    throw new IOException($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                                }

                                var contentLength = resp.Content.Headers.ContentLength;

                                var dir = Path.GetDirectoryName(destPath);
                                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                                using (var responseStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                using (var fileStream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                                {
                                    var buffer = new byte[81920];
                                    long totalRead = 0;
                                    int read;
                                    if (!contentLength.HasValue)
                                    {
                                        progress?.Report(-1.0);
                                    }
                                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, attemptCts.Token).ConfigureAwait(false)) > 0)
                                    {
                                        await fileStream.WriteAsync(buffer, 0, read, attemptCts.Token).ConfigureAwait(false);
                                        totalRead += read;
                                        if (contentLength.HasValue)
                                        {
                                            progress?.Report(Math.Min(1.0, (double)totalRead / contentLength.Value));
                                        }
                                        if (attemptCts.Token.IsCancellationRequested) return false;
                                    }
                                }

                                // Replace final file
                                try { if (File.Exists(destPath)) File.Delete(destPath); } catch { }
                                File.Move(tmpPath, destPath);
                                progress?.Report(1.0);
                                return true;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // If external cancellation requested => return false immediately.
                            if (cancellationToken.IsCancellationRequested) return false;

                            // Otherwise treat as timeout for this attempt; retry if attempts left.
                            if (attempt < maxAttempts)
                            {
                                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }

                                var delayMs = baseDelay.Value.TotalMilliseconds * Math.Pow(2, attempt - 1);
                                var jitterMs = rng.Next(0, 300);
                                var delay = TimeSpan.FromMilliseconds(delayMs + jitterMs);
                                try { await Task.Delay(delay, cancellationToken).ConfigureAwait(false); } catch { return false; }
                                continue;
                            }
                            return false;
                        }
                        catch (Exception) when (attempt < maxAttempts)
                        {
                            // failed but will retry
                            try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }

                            var delayMs = baseDelay.Value.TotalMilliseconds * Math.Pow(2, attempt - 1);
                            var jitterMs = rng.Next(0, 300);
                            var delay = TimeSpan.FromMilliseconds(delayMs + jitterMs);
                            try { await Task.Delay(delay, cancellationToken).ConfigureAwait(false); } catch { return false; }
                            continue;
                        }
                        // 最后一次异常会导致循环结束并返回 false below
                    }
                }

                return false;
            }
            finally
            {
                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
                if (disposeClient)
                {
                    try { httpClient.Dispose(); } catch { }
                }
            }
        }

    }
}
