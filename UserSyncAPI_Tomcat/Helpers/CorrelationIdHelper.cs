namespace UserSyncAPI_Tomcat.Helpers
{
    public class CorrelationIdHelper
    {



        public static string GetOrCreateCorrelationId(HttpRequest request)
        {
            const string headerName = Common.Constants.Headers.CORRELATION_ID;

            // Header exists and has a value
            if (request.Headers.TryGetValue(headerName, out var values) && !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
            {
                return values.First();
            }

            // Create new correlation id
            var correlationId = Guid.NewGuid().ToString();

            // Add to request headers (context)
            request.Headers[headerName] = correlationId;

            return correlationId;
        }
    }
}