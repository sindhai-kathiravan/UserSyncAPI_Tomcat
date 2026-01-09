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

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ===============================
            // 1. Correlation Id
            // ===============================
            string correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(context.Request);
            context.Items["CorrelationId"] = correlationId;

            // SET headers (do NOT append)
            context.Request.Headers[Common.Constants.Headers.CORRELATION_ID] = correlationId;
            context.Response.Headers[Common.Constants.Headers.CORRELATION_ID] = correlationId;

            // ===============================
            // 2. Request Logging
            // ===============================
            context.Request.EnableBuffering();

            string requestBody = await ReadRequestBodyAsync(context.Request);
            context.Request.Body.Position = 0;

            Logger.Log(
                $"{correlationId} | Incoming Request | " +
                $"Method: {context.Request.Method} | " +
                $"Path: {context.Request.Path} | " +
                $"Body: {SanitizePassword(requestBody)}");

            // ===============================
            // 3. Capture Response
            // ===============================
            var originalResponseBody = context.Response.Body;

            await using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context); // ⚠️ MUST be called exactly once

            // ===============================
            // 4. Read & Log Response
            // ===============================
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

            // Optional: limit response size
            if (!string.IsNullOrEmpty(responseBody) && responseBody.Length > 5000)
                responseBody = responseBody.Substring(0, 5000) + "...(truncated)";

            Logger.Log(
                $"{correlationId} | Outgoing Response | " +
                $"StatusCode: {context.Response.StatusCode} | " +
                $"Body: {responseBody}");

            // ===============================
            // 5. Restore Response Stream
            // ===============================
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);

            context.Response.Body = originalResponseBody; // 🔥 CRITICAL
        }

        // ===============================
        // Helpers
        // ===============================
        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            return await reader.ReadToEndAsync();
        }

        private static string SanitizePassword(string body)
        {
            if (string.IsNullOrEmpty(body))
                return body;

            return Regex.Replace(
                body,
                "\"password\"\\s*:\\s*\"(.*?)\"",
                "\"password\":\"******\"",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }

}