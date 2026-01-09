using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UserSyncAPI_Tomcat.Common;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Models;
namespace UserSyncAPI_Tomcat.Authentication
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly IConfiguration _config;

        public ValidateModelAttribute(IConfiguration config)
        {
            _config = config;
        }
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var httpContext = actionContext.HttpContext;

            HttpRequest request = httpContext.Request;
            string correlationId = Guid.NewGuid().ToString();

            // Add to request headers (context)
            request.Headers[Common.Constants.Headers.CORRELATION_ID] = correlationId;


            ///////////
            var controller = actionContext.RouteData.Values["controller"];
            var action = actionContext.RouteData.Values["action"];

            var ipAddress =
                httpContext.Connection.RemoteIpAddress?.ToString();

            var userAgent =
                request.Headers["User-Agent"].ToString();

            var queryParams = request.Query
                .ToDictionary(q => q.Key, q => q.Value.ToString());

            var queryJson = JsonConvert.SerializeObject(
                queryParams,
                Formatting.Indented
            );

            var headers = request.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            var headersJson = JsonConvert.SerializeObject(
                headers,
                Formatting.Indented
            );
            string? reqBody = MaskSensitive(actionContext.ActionArguments);
            Logger.Log(
                $"API Request | CorrelationId:{correlationId} | Method: {request.Method} | Controller: {controller} | Action: {action} |IP: {ipAddress} | UserAgent: {userAgent} | Query: {queryJson} | Headers: {headersJson} | Body: {reqBody}"
            );

            // string correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(httpContext.Request);
            string method = httpContext.Request.Method;
            var model = actionContext.ActionArguments.Values.FirstOrDefault();
            var actionName = actionContext.ActionDescriptor.RouteValues["action"];
            if (!string.Equals(actionName, Common.Constants.ActionNames.GetAllUser, StringComparison.OrdinalIgnoreCase))
            {
                if (model == null)
                {
                    ApiResponse<EmptyData> response = new ApiResponse<EmptyData>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Status = HttpStatusCode.BadRequest.ToString(),
                        Message = Common.Constants.Messages.REQUEST_BODY_IS_NULL,
                        Error = Common.Constants.Errors.ERR_NULL_BODY,
                        Data = new EmptyData(),
                        Success = false,
                        CorrelationId = correlationId
                    };
                    actionContext.Result = new JsonResult(response)
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };

                    PrintResponse(response); // adjust based on your implementation
                }
            }
            if (!actionContext.ModelState.IsValid)
            {
                var errors = actionContext.ModelState
                                .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                                .ToDictionary(
                                                ms => ms.Key,
                                                ms => ms.Value!.Errors
                                                               .Select(e => e.ErrorMessage)
                                                               .ToArray()
                                              );

                var response = new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Status = HttpStatusCode.BadRequest.ToString(),
                    Message = Common.Constants.Messages.THE_REQUEST_IS_INVALID,
                    Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                    Data = actionContext.ModelState,
                    Success = false,
                    CorrelationId = correlationId
                };
                actionContext.Result = new JsonResult(response)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                PrintResponse(response);
                return;
            }
            else
            {
                var createUserRequest = model as CreateUserRequest;
                var updateUserRequest = model as UpdateUserRequest;
                var deleteUserRequest = model as DeleteUserRequest;
                if (createUserRequest != null || updateUserRequest != null || deleteUserRequest != null)
                {
                    if (((createUserRequest != null) && ((createUserRequest.TargetFactories == null) || (!createUserRequest.TargetFactories.Any())))
                        || ((updateUserRequest != null) && ((updateUserRequest.TargetFactories == null || !updateUserRequest.TargetFactories.Any()))))
                    {
                        var response = new ApiResponse<EmptyData>
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Status = HttpStatusCode.BadRequest.ToString(),
                            Message = Common.Constants.Messages.AT_LEAST_ONE_TARGET_DATABASE_MUST_BE_SPECIFIED,
                            Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                            Data = new EmptyData(),
                            Success = false,
                            CorrelationId = correlationId
                        };
                        actionContext.Result = new JsonResult(response)
                        {
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                        PrintResponse(response);
                        return;
                    }
                    string? sourceSystem = createUserRequest?.SourceSystem
                                       ?? updateUserRequest?.SourceSystem
                                       ?? deleteUserRequest?.SourceSystem;
                    var sourceSystemKey = _config.GetConnectionString(sourceSystem);
                    if (sourceSystemKey == null)
                    {
                        var response = new ApiResponse<ValidationResponseData>
                        {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Status = HttpStatusCode.BadRequest.ToString(),
                            Message = Common.Constants.Messages.INVALID_SOURCE_SYSTEM,
                            Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                            Data = new ValidationResponseData
                            {
                                ValidationMessage = string.Format(Common.Constants.Messages.THE_SOURCE_SYSTEM_XXXX_DOES_NOT_EXIST_IN_THE_SYSTEM_LIST, sourceSystem)
                            },
                            Success = false,
                            CorrelationId = correlationId,
                        };
                        actionContext.Result = new JsonResult(response)
                        {
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                        PrintResponse(response);
                        return;
                    }
                    if (createUserRequest != null || updateUserRequest != null)
                    {


                        List<string>? targetDatabases = createUserRequest?.TargetFactories
                                                ?? updateUserRequest?.TargetFactories;
                        string[] validKeys = _config
                                            .GetSection("ConnectionStrings")
                                            .GetChildren()
                                            .Select(x => x.Key)
                                            .ToArray();
                        var invalidKeys = targetDatabases?.Where(key => !validKeys.Contains(key)).ToList();
                        if (invalidKeys.Count != 0)
                        {
                            var errorObj = new
                            {
                                ValidationMessage = invalidKeys.Select(key => $"Invalid database key: {key}").ToList()
                            };
                            string json = JsonConvert.SerializeObject(errorObj);
                            var response = new ApiResponse<object>
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Status = HttpStatusCode.BadRequest.ToString(),
                                Message = Common.Constants.Messages.INVALID_DATABASE_KEY_FOUND,
                                Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                Data = errorObj,
                                Success = false,
                                CorrelationId = correlationId,
                            };
                            actionContext.Result = new JsonResult(response)
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            PrintResponse(response);
                            return;
                        }
                    }
                    if (updateUserRequest != null || deleteUserRequest != null)
                    {
                        int? userId = updateUserRequest?.UserId
                                  ?? deleteUserRequest?.UserId;
                        if (userId <= 0)
                        {
                            var response = new ApiResponse<ValidationResponseData>
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Status = HttpStatusCode.BadRequest.ToString(),
                                Message = Common.Constants.Messages.INVALID_USER_ID,
                                Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                Data = new ValidationResponseData
                                {
                                    ValidationMessage = string.Format(Common.Constants.Messages.THE_USER_ID_XX_IS_INVALID, updateUserRequest.UserId)
                                },
                                Success = false,
                                CorrelationId = correlationId,
                            };
                            actionContext.Result = new JsonResult(response)
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            PrintResponse(response);
                            return;
                        }
                        bool idUnavailable = CheckUserExistsInAllDatabases(userId);
                        if (idUnavailable)
                        {
                            var errorObj = new
                            {
                                ValidationMessage = $"The user id {userId} does not exist in the database"
                            };
                            string json = JsonConvert.SerializeObject(errorObj);
                            var response = new ApiResponse<object>
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Status = HttpStatusCode.BadRequest.ToString(),
                                Message = Common.Constants.Messages.USER_ID_DOES_NOT_EXIST,
                                Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                Data = JToken.Parse(json),
                                Success = false,
                                CorrelationId = correlationId,
                            };
                            actionContext.Result = new JsonResult(response)
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            PrintResponse(response);
                            return;
                        }
                    }
                    if (createUserRequest != null || updateUserRequest != null)
                    {
                        bool? AllDatabaseSameUserId = Convert.ToBoolean(_config["Validations:AllDatabaseSameUserId"]);
                        if ((bool)AllDatabaseSameUserId && createUserRequest != null)
                        {
                            var dbMaxIds = GetUsersMaxIds();
                            var distinctValues = new HashSet<int?>(dbMaxIds.Values);
                            if (distinctValues.Count > 1)
                            {
                                List<object> maxUserIdList = new List<object>();
                                foreach (var kvp in dbMaxIds)
                                {
                                    maxUserIdList.Add(new { Database = kvp.Key, MaxUserId = $"{(kvp.Value.HasValue ? kvp.Value.ToString() : "NULL")}" });
                                }
                                var response = new ApiResponse<object>
                                {
                                    StatusCode = (int)HttpStatusCode.BadRequest,
                                    Status = HttpStatusCode.BadRequest.ToString(),
                                    Message = Common.Constants.Messages.MAX_IDS_ARE_NOT_THE_SAME_ACROSS_DATABASES,
                                    Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                    Data = maxUserIdList,
                                    Success = false,
                                    CorrelationId = correlationId,
                                };
                                actionContext.Result = new JsonResult(response)
                                {
                                    StatusCode = StatusCodes.Status400BadRequest
                                };
                                PrintResponse(response);
                                return;
                            }
                        }
                        string? userName = createUserRequest?.UserName
                                       ?? updateUserRequest?.UserName;
                        string? domain = _config["LdapSettings:Server"];

                        if (!IsUserPresentInDomain(userName, domain))
                        {
                            var response = new ApiResponse<object>
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Status = HttpStatusCode.BadRequest.ToString(),
                                Message = Common.Constants.Messages.INVALID_USER_ID,
                                Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                Data = new { ValidationMessage = $"The user ID '{userName}'is not registered in the organization's domain." },
                                Success = false,
                                CorrelationId = correlationId,
                            };
                            actionContext.Result = new JsonResult(response)
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            PrintResponse(response);
                            return;
                        }

                        int existingUserNameCount = CheckUserNameExists(userName);
                        if (createUserRequest != null && existingUserNameCount > 0)
                        {
                            var response = new ApiResponse<object>
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Status = HttpStatusCode.BadRequest.ToString(),
                                Message = Common.Constants.Messages.USERNAME_ALREADY_EXIST,
                                Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                Data = new { ValidationMessage = $"The username '{userName}' already exist." },
                                Success = false,
                                CorrelationId = correlationId,
                            };
                            actionContext.Result = new JsonResult(response)
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            PrintResponse(response);
                            return;
                        }
                        int? userId = GetUserIdForUsername(userName);
                        if (updateUserRequest != null && userId != null && userId != updateUserRequest.UserId)
                        {
                            var response = new ApiResponse<object>
                            {
                                StatusCode = (int)HttpStatusCode.BadRequest,
                                Status = HttpStatusCode.BadRequest.ToString(),
                                Message = Common.Constants.Messages.USERNAME_ALREADY_EXIST,
                                Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                Data = new { ValidationMessage = $"The username '{userName}' already exist." },
                                Success = false,
                                CorrelationId = correlationId,
                            };
                            actionContext.Result = new JsonResult(response)
                            {
                                StatusCode = StatusCodes.Status400BadRequest
                            };
                            PrintResponse(response);
                            return;
                        }
                    }
                }
                else
                {
                    if (method == Common.Constants.Methods.GET)
                    {
                        if (!string.Equals(actionName, Common.Constants.ActionNames.GetAllUser, StringComparison.OrdinalIgnoreCase))
                        {
                            var userIdKey = queryParams.Keys.FirstOrDefault(k => k.Equals(Common.Constants.QueryStrings.UserId, StringComparison.OrdinalIgnoreCase));
                            if (userIdKey == null || !queryParams.TryGetValue(userIdKey, out var userIdValue) || string.IsNullOrWhiteSpace(userIdValue))
                            {
                                var response = new ApiResponse<object>
                                {
                                    StatusCode = (int)HttpStatusCode.BadRequest,
                                    Status = HttpStatusCode.BadRequest.ToString(),
                                    Message = Common.Constants.Messages.USERID_QUERY_PARAMETER_IS_REQUIRED,
                                    Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                    Data = new { ValidationMessage = Common.Constants.Messages.USERID_QUERY_PARAMETER_IS_REQUIRED },
                                    Success = false,
                                    CorrelationId = correlationId,
                                };
                                actionContext.Result = new JsonResult(response)
                                {
                                    StatusCode = StatusCodes.Status400BadRequest
                                };
                                PrintResponse(response);
                                return;
                            }
                        }
                        var sourceSystemdKey = queryParams.Keys.FirstOrDefault(k => k.Equals(Common.Constants.QueryStrings.SourceSystem, StringComparison.OrdinalIgnoreCase));
                        string? sourceSystemValue = null;
                        if (sourceSystemdKey != null)
                        {
                            queryParams.TryGetValue(sourceSystemdKey, out sourceSystemValue);
                            if (!string.IsNullOrWhiteSpace(sourceSystemValue))
                            {
                                var sourceSystemKey = _config.GetConnectionString(sourceSystemValue);
                                if (sourceSystemKey == null)
                                {
                                    var response = new ApiResponse<ValidationResponseData>
                                    {
                                        StatusCode = (int)HttpStatusCode.BadRequest,
                                        Status = HttpStatusCode.BadRequest.ToString(),
                                        Message = Common.Constants.Messages.INVALID_SOURCE_SYSTEM,
                                        Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                                        Data = new ValidationResponseData
                                        {
                                            ValidationMessage = string.Format(Common.Constants.Messages.THE_SOURCE_SYSTEM_XXXX_DOES_NOT_EXIST_IN_THE_SYSTEM_LIST, sourceSystemValue)
                                        },
                                        Success = false,
                                        CorrelationId = correlationId,
                                    };
                                    actionContext.Result = new JsonResult(response)
                                    {
                                        StatusCode = StatusCodes.Status400BadRequest
                                    };
                                    PrintResponse(response);
                                    return;
                                }
                            }
                        }
                    }
                }
                if ((string.Equals(actionName, Common.Constants.ActionNames.Login, StringComparison.OrdinalIgnoreCase)) || (string.Equals(actionName, Common.Constants.ActionNames.DomainLogin, StringComparison.OrdinalIgnoreCase)))
                {
                    var loginRequest = model as LoginRequest;

                    var sourceSystemKey = _config.GetConnectionString(loginRequest.SourceSystem);
                    if (sourceSystemKey == null)
                    {
                        var response = new ApiResponse<object>
                        {
                            Success = false,
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Status = HttpStatusCode.BadRequest.ToString(),
                            Message = Common.Constants.Messages.INVALID_SOURCE_SYSTEM,
                            Error = Common.Constants.Errors.ERR_VALIDATION_FAILUED,
                            Data = new { ValidationMessage = string.Format(Common.Constants.Messages.THE_SOURCE_SYSTEM_XXXX_DOES_NOT_EXIST_IN_THE_SYSTEM_LIST, loginRequest.SourceSystem) },
                            CorrelationId = correlationId,
                        };
                        actionContext.Result = new JsonResult(response)
                        {
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                        PrintResponse(response);
                        return;
                    }
                }
                Logger.Log("Validation succeeded.");
                return;
            }
        }

        private string? SanitizePassword(string body)
        {
            if (string.IsNullOrEmpty(body))
                return body;

            return Regex.Replace(
                body,
                "\"password\"\\s*:\\s*\"(.*?)\"",
                "\"password\":\"******\"",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        }

        private bool IsUserPresentInDomain(string? userName, string? domain)
        {
            try
            {
                if (userName == null)
                {
                    return false;
                }
                if (!long.TryParse(userName, out _))
                {

                    using var context = new PrincipalContext(ContextType.Domain, domain);   // your domain
                    var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);
                    return user != null;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private Dictionary<string, int?> GetUsersMaxIds()
        {
            var dbMaxIds = new Dictionary<string, int?>();
            //var config = new ConfigurationBuilder()
            //                        .SetBasePath(AppContext.BaseDirectory)
            //                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //                        .Build();

            // Get all connection strings
            var connectionStringsSection = _config.GetSection("ConnectionStrings");
            var allConnectionStrings = connectionStringsSection
                .GetChildren()
                .ToDictionary(x => x.Key, x => x.Value);
            foreach (var cs in allConnectionStrings)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(cs.Value))
                    {
                        conn.Open();
                        string sql = "SELECT MAX(user_id) FROM Users";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            object result = cmd.ExecuteScalar();
                            int? maxId = null;
                            if (result == DBNull.Value)
                            {
                                maxId = null;
                            }
                            else
                            {
                                maxId = Convert.ToInt32(result);
                            }
                            dbMaxIds[cs.Key] = maxId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {cs.Key}: {ex.Message}");
                    dbMaxIds[cs.Key] = null;
                }
            }

            return dbMaxIds;
        }

        private int? GetUserIdForUsername(string userName)
        {
            using (var conn = DbConnectionFactory.GetDefaultConnection())
            {
                using (var cmd = new SqlCommand("SELECT user_id FROM Users WHERE deleted = 0 AND user_name = @UserName", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["user_id"]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        private int CheckUserNameExists(string userName)
        {
            using (var conn = DbConnectionFactory.GetDefaultConnection())
            {
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE deleted = 0 AND user_name = @UserName", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private bool CheckUserExistsInAllDatabases(int? userId)
        {
            using (var conn = DbConnectionFactory.GetDefaultConnection())
            {
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE deleted = 0 AND user_id = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count == 0;

                }
            }
        }
        private void PrintResponse(object response)
        {
            string prettyJson = JsonConvert.SerializeObject(response, Formatting.Indented);
            Logger.Log("Validation failed.");
            Logger.Log($"Response: {prettyJson}");
        }
        private string ReadRequestBody(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            return reader.ReadToEnd();
        }
        private static string? MaskSensitive(object body)
        {
            if (body == null) return null;

            var json = JsonConvert.SerializeObject(body);

            var jToken = JToken.Parse(json);

            MaskToken(jToken);

            return jToken.ToString(Formatting.Indented);
        }

        private static void MaskToken(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (var property in token.Children<JProperty>())
                {
                    if (IsSensitiveKey(property.Name))
                    {
                        property.Value = "***";
                    }
                    else
                    {
                        MaskToken(property.Value);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    MaskToken(child);
                }
            }
        }

        private static bool IsSensitiveKey(string key)
        {
            return key.Equals("password", StringComparison.OrdinalIgnoreCase)
                || key.Equals("pwd", StringComparison.OrdinalIgnoreCase)
                || key.Equals("token", StringComparison.OrdinalIgnoreCase)
                || key.Equals("authorization", StringComparison.OrdinalIgnoreCase)
                || key.Equals("secret", StringComparison.OrdinalIgnoreCase);
        }
        //public override void OnActionExecuted(ActionExecutedContext context)
        //{
        //    Logger.Log(string.Format("API Response | StatusCode: {StatusCode} | Controller: {Controller} | Action: {Action}",
        //        context.HttpContext.Response.StatusCode,
        //        context.RouteData.Values["controller"],
        //        context.RouteData.Values["action"]
        //    ));
        //}

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var controller = context.RouteData.Values["controller"];
            var action = context.RouteData.Values["action"];
            var statusCode = context.HttpContext.Response.StatusCode;

            object? responseBody = null;

            switch (context.Result)
            {
                case ObjectResult objectResult:
                    responseBody = objectResult.Value;
                    break;

                case JsonResult jsonResult:
                    responseBody = jsonResult.Value;
                    break;

                case ContentResult contentResult:
                    responseBody = contentResult.Content;
                    break;

                case EmptyResult:
                    responseBody = null;
                    break;
            }

            var maskedResponse = MaskSensitive(responseBody);

            Logger.Log(
                $"API Response | StatusCode: {statusCode} | Controller: {controller} | Action: {action} | Response: {maskedResponse}"
            );
        }

    }
}