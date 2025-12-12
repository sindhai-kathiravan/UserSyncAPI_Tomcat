using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace UserSyncAPI_Tomcat.Filter
{
    public class ResponseLoggingFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                var content = objectResult.Value;

                string responseJson = JsonConvert.SerializeObject(content);

                string prettyJson;
                try
                {
                    prettyJson = JToken.Parse(responseJson).ToString(Formatting.Indented);
                }
                catch
                {
                    prettyJson = responseJson;
                }

                Logger.Log($"Response: {prettyJson}");
            }

            base.OnActionExecuted(context);
        }
    }
}