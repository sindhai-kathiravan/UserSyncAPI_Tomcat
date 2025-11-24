namespace UserSyncAPI_Tomcat.Helpers
{
    public class CorrelationIdHelper
    {

        public static string GetOrCreateCorrelationId(HttpRequest request)
        {
            const string headerName = Common.Constants.Headers.CORRELATION_ID;

            if (request.Headers.TryGetValue(headerName, out var values))
            {
                // Take the first value or generate a new GUID if empty
                return values.FirstOrDefault() ?? Guid.NewGuid().ToString();
            }

            return Guid.NewGuid().ToString();
        }

    }
}