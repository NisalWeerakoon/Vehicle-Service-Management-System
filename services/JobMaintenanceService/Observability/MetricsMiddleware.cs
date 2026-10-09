using System.Diagnostics;
using System.Text;

namespace Observability;

public sealed class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;

    private static long _requestCount;
    private static long _failureCount;
    private static long _totalResponseTimeMs;

    public MetricsMiddleware(
        RequestDelegate next,
        ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);

            if (context.Response.StatusCode >= 400)
            {
                Interlocked.Increment(ref _failureCount);

                _logger.LogWarning(
                    "HTTP request failed. Method={Method}, Path={Path}, StatusCode={StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failureCount);

            _logger.LogError(
                ex,
                "Unhandled exception while processing HTTP request. Method={Method}, Path={Path}",
                context.Request.Method,
                context.Request.Path);

            throw;
        }
        finally
        {
            stopwatch.Stop();

            Interlocked.Increment(ref _requestCount);
            Interlocked.Add(
                ref _totalResponseTimeMs,
                stopwatch.ElapsedMilliseconds);
        }
    }

    public static string GetMetrics()
    {
        var requests = Interlocked.Read(ref _requestCount);
        var failures = Interlocked.Read(ref _failureCount);
        var totalResponseTime = Interlocked.Read(ref _totalResponseTimeMs);

        var averageResponseTime =
            requests > 0
                ? (double)totalResponseTime / requests
                : 0;

        var builder = new StringBuilder();

        builder.AppendLine("# HELP vsc_http_requests_total Total HTTP requests.");
        builder.AppendLine("# TYPE vsc_http_requests_total counter");
        builder.AppendLine($"vsc_http_requests_total {requests}");

        builder.AppendLine("# HELP vsc_http_failures_total Total HTTP requests returning errors.");
        builder.AppendLine("# TYPE vsc_http_failures_total counter");
        builder.AppendLine($"vsc_http_failures_total {failures}");

        builder.AppendLine("# HELP vsc_http_response_time_ms_total Total HTTP response time in milliseconds.");
        builder.AppendLine("# TYPE vsc_http_response_time_ms_total counter");
        builder.AppendLine($"vsc_http_response_time_ms_total {totalResponseTime}");

        builder.AppendLine("# HELP vsc_http_response_time_ms_average Average HTTP response time in milliseconds.");
        builder.AppendLine("# TYPE vsc_http_response_time_ms_average gauge");
        builder.AppendLine($"vsc_http_response_time_ms_average {averageResponseTime.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        return builder.ToString();
    }
}
