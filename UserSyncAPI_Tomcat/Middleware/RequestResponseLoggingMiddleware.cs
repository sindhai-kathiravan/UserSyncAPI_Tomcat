using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UserSyncAPI_Tomcat.Helpers;
namespace UserSyncAPI_Tomcat.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1️⃣ Get or Create CorrelationId
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(context.Request);
            context.Items["CorrelationId"] = correlationId;
            context.Request.Headers.Append(Common.Constants.Headers.CORRELATION_ID, correlationId);
            context.Response.Headers.Append(Common.Constants.Headers.CORRELATION_ID, correlationId);


            // Client IP
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            // Request Origin
            var origin = context.Request.Headers["Origin"].FirstOrDefault();

            // Referrer
            var referer = context.Request.Headers["Referer"].FirstOrDefault();

            // Request URI
            var requestUri = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";


            // 2️⃣ Log Request
            context.Request.EnableBuffering(); // Allow reading body multiple times
            string requestBody = await ReadRequestBody(context);
            _logger.LogInformation("New Incoming Request | CorrelationId: {CorrelationId} | Path: {Path} | Body: {Body}",
                correlationId, context.Request.Path, requestBody);
            context.Request.Body.Position = 0;
            Logger.Log($"{correlationId} : ===== New Incoming Request =====\n\tIP Address : {clientIp}\n\tOrigin : {origin}\n\tReferrer : {referer}\n\tRequestUri : {requestUri}");
            Logger.Log($"{correlationId} : Path {context.Request.Path}");
           // var json = JsonSerializer.Serialize(requestBody);

            Logger.Log($"{correlationId} : Body {SanitizePassword(requestBody)}");
            // 3️⃣ Capture Response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context); // Execute next middleware

            // 4️⃣ Log Response
            string responseBody = await ReadResponseBody(context);
            _logger.LogInformation("Outgoing Response | CorrelationId: {CorrelationId} | Status: {Status} | Body: {Body}",
                correlationId, context.Response.StatusCode, responseBody);

            // Write back to original response stream
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        string SanitizePassword(string requestBody)
        {
            if (string.IsNullOrEmpty(requestBody))
                return requestBody;

            // Matches: "password": "anything"
            return Regex.Replace(
                requestBody,
                "\"password\"\\s*:\\s*\"(.*?)\"",
                "\"password\":\"******\"",
                RegexOptions.IgnoreCase
            );
        }
        private async Task<string> ReadRequestBody(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private async Task<string> ReadResponseBody(HttpContext context)
        {
            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
    }
}