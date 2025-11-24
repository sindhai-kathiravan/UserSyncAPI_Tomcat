using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;
using UserSyncAPI_Tomcat.Authentication;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Models;
using UserSyncAPI_Tomcat.Security;

namespace UserSyncAPI_Tomcat.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class AuthenticationController : ControllerBase
    {
        private readonly LdapSettings _ldapSettings;

        public AuthenticationController(IOptions<LdapSettings> ldapOptions)
        {
            _ldapSettings = ldapOptions.Value;
        }
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            ApiResponse<object>? apiResponse = null;
            string? message = null;
            string? error = null;
            bool success = false;
            try
            {
                var config = new ConfigurationBuilder()
                                 .SetBasePath(AppContext.BaseDirectory)
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                 .Build();

                string? cnxnStr = config?.GetConnectionString(request.SourceSystem);

                using (SqlConnection connection = new SqlConnection(cnxnStr))
                {
                    connection.Open();
                    var sql = @"
                               IF EXISTS (SELECT 1 FROM Users U    
                                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
                                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
                                    BEGIN
                                        UPDATE Users 
                                        SET user_loggedin = 1, lastlogin = GETDATE() 
                                        WHERE user_name = @UserName;
                                        SELECT 1;
                                    END
                                    ELSE
                                    BEGIN
                                        UPDATE Users 
                                        SET fattempt = fattempt + 1 
                                        WHERE user_name = @UserName;
                                        SELECT 0;
                                    END";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", request.Username);
                        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
                        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);

                        int result = (int)command.ExecuteScalar();
                        if (result == 1)
                        {
                            success = true;
                            message = Common.Constants.Messages.USER_DATABASE_AUTHENTICATION_SUCCESSFUL;
                        }
                        else
                        {
                            success = false;
                            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
                            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
                        }
                    }
                }
                apiResponse = new ApiResponse<object>
                {
                    Success = success,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = message,
                    Data = null,
                    Error = error,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);

            }
            catch (Exception ex)
            {
                Logger.Log($"Error in Login: {ex.Message}");
                Logger.Log($"StackTrace in Login: {ex.StackTrace}");
                apiResponse = new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Status = HttpStatusCode.InternalServerError.ToString(),
                    Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
                    Data = null,
                    Error = Common.Constants.Errors.ERR_INTERNAL_SERVER,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
            }
        }

        //[HttpPost]
        //[Route("domain-login")]
        //public IActionResult DomainLogin([FromBody] LoginRequest request)
        //{
        //    ApiResponse<object>? apiResponse = null;
        //    string? message = null;
        //    string? error = null;
        //    bool success = false;
        //    try
        //    {
        //        var ldapServer = _ldapSettings.Server;
        //        var credential = new NetworkCredential(request.Username, request.Password);
        //        using var connection = new LdapConnection(ldapServer);
        //        connection.Credential = credential;
        //        connection.Bind();


        //        //var config = new ConfigurationBuilder()
        //        //                 .SetBasePath(AppContext.BaseDirectory)
        //        //                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //        //                 .Build();

        //        //string? cnxnStr = config?.GetConnectionString(request.SourceSystem);

        //        //using (SqlConnection connection = new SqlConnection(cnxnStr))
        //        //{
        //        //    connection.Open();
        //        //    var sql = @"
        //        //               IF EXISTS (SELECT 1 FROM Users U    
        //        //                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
        //        //                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
        //        //                    BEGIN
        //        //                        UPDATE Users 
        //        //                        SET user_loggedin = 1, lastlogin = GETDATE() 
        //        //                        WHERE user_name = @UserName;
        //        //                        SELECT 1;
        //        //                    END
        //        //                    ELSE
        //        //                    BEGIN
        //        //                        UPDATE Users 
        //        //                        SET fattempt = fattempt + 1 
        //        //                        WHERE user_name = @UserName;
        //        //                        SELECT 0;
        //        //                    END";
        //        //    using (var command = new SqlCommand(sql, connection))
        //        //    {
        //        //        command.Parameters.AddWithValue("@UserName", request.Username);
        //        //        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
        //        //        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);

        //        //        int result = (int)command.ExecuteScalar();
        //        //        if (result == 1)
        //        //        {
        //        //            success = true;
        //        //            message = Common.Constants.Messages.USER_AUTHENTICATION_SUCCESSFUL;
        //        //        }
        //        //        else
        //        //        {
        //        //            success = false;
        //        //            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
        //        //            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
        //        //        }
        //        //    }
        //        //}
        //        apiResponse = new ApiResponse<object>
        //        {
        //            Success = success,
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Status = HttpStatusCode.OK.ToString(),
        //            Message = message,
        //            Data = null,
        //            Error = error,
        //            CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
        //        };
        //        return StatusCode(StatusCodes.Status200OK, apiResponse);
        //    }
        //    catch (LdapException ex)
        //    {
        //        Logger.Log($"Error in DomainLogin: {ex.Message}");
        //        Logger.Log($"StackTrace in DomainLogin: {ex.StackTrace}");
        //        var status= HttpStatusCode.InternalServerError.ToString();
        //        var statusCode = (int)HttpStatusCode.OK;
        //        success = false;
        //        if (ex.Message == "The supplied credential is invalid.")
        //        {
        //            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
        //            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
        //            status = HttpStatusCode.OK.ToString();
        //            statusCode = (int)HttpStatusCode.OK;
        //        }
        //        else
        //        {
        //            message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED;
        //            error = Common.Constants.Errors.ERR_INTERNAL_SERVER;
        //            status = HttpStatusCode.InternalServerError.ToString();
        //            statusCode = (int)HttpStatusCode.InternalServerError;
        //        }
        //        apiResponse = new ApiResponse<object>
        //        {
        //            Success = success,
        //            StatusCode = statusCode,
        //            Status = status,
        //            Message = message,
        //            Data = null,
        //            Error = error,
        //            CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
        //        };
        //        return StatusCode(statusCode, apiResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log($"Error in Login: {ex.Message}");
        //        Logger.Log($"StackTrace in Login: {ex.StackTrace}");
        //        apiResponse = new ApiResponse<object>
        //        {
        //            Success = false,
        //            StatusCode = (int)HttpStatusCode.InternalServerError,
        //            Status = HttpStatusCode.InternalServerError.ToString(),
        //            Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
        //            Data = null,
        //            Error = Common.Constants.Errors.ERR_INTERNAL_SERVER,
        //            CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
        //        };
        //        return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
        //    }
        //}

        [HttpPost]
        [Route("domain-login")]
        public IActionResult DomainLogin([FromBody] LoginRequest request)
        {
            ApiResponse<object>? apiResponse = null;
            string? message = null;
            string? error = null;
            bool success = false;

            bool adAvailable = IsADAvailable();

            if (adAvailable)
            {
                bool adSuccess = TryADAuthentication(request);

                if (adSuccess)
                {
                    apiResponse = new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = (int)HttpStatusCode.OK,
                        Status = HttpStatusCode.OK.ToString(),
                        Message = Common.Constants.Messages.USER_DOMAIN_AUTHENTICATION_SUCCESSFUL,
                        Data = null,
                        Error = error,
                        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                    };
                    return StatusCode(StatusCodes.Status200OK, apiResponse);
                }
                else
                {
                    // AD reachable but login failed → Fallback to DB
                    apiResponse = new ApiResponse<object>
                    {
                        Success = false,
                        StatusCode = (int)HttpStatusCode.OK,
                        Status = HttpStatusCode.OK.ToString(),
                        Message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM,
                        Data = null,
                        Error = Common.Constants.Errors.ERR_LOGIN_FAILED,
                        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                    };
                    return StatusCode(StatusCodes.Status200OK, apiResponse);
                }
            }
            else
            {
                // AD is DOWN → Direct DB login
                return Login(request);
            }
            //try
            //{
            //    var ldapServer = _ldapSettings.Server;
            //    var credential = new NetworkCredential(request.Username, request.Password);
            //    using var connection = new LdapConnection(ldapServer);
            //    connection.Credential = credential;
            //    connection.Bind();


            //    //var config = new ConfigurationBuilder()
            //    //                 .SetBasePath(AppContext.BaseDirectory)
            //    //                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    //                 .Build();

            //    //string? cnxnStr = config?.GetConnectionString(request.SourceSystem);

            //    //using (SqlConnection connection = new SqlConnection(cnxnStr))
            //    //{
            //    //    connection.Open();
            //    //    var sql = @"
            //    //               IF EXISTS (SELECT 1 FROM Users U    
            //    //                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
            //    //                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
            //    //                    BEGIN
            //    //                        UPDATE Users 
            //    //                        SET user_loggedin = 1, lastlogin = GETDATE() 
            //    //                        WHERE user_name = @UserName;
            //    //                        SELECT 1;
            //    //                    END
            //    //                    ELSE
            //    //                    BEGIN
            //    //                        UPDATE Users 
            //    //                        SET fattempt = fattempt + 1 
            //    //                        WHERE user_name = @UserName;
            //    //                        SELECT 0;
            //    //                    END";
            //    //    using (var command = new SqlCommand(sql, connection))
            //    //    {
            //    //        command.Parameters.AddWithValue("@UserName", request.Username);
            //    //        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
            //    //        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);

            //    //        int result = (int)command.ExecuteScalar();
            //    //        if (result == 1)
            //    //        {
            //    //            success = true;
            //    //            message = Common.Constants.Messages.USER_AUTHENTICATION_SUCCESSFUL;
            //    //        }
            //    //        else
            //    //        {
            //    //            success = false;
            //    //            message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
            //    //            error = Common.Constants.Errors.ERR_LOGIN_FAILED;
            //    //        }
            //    //    }
            //    //}
            //    apiResponse = new ApiResponse<object>
            //    {
            //        Success = success,
            //        StatusCode = (int)HttpStatusCode.OK,
            //        Status = HttpStatusCode.OK.ToString(),
            //        Message = message,
            //        Data = null,
            //        Error = error,
            //        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
            //    };
            //    return StatusCode(StatusCodes.Status200OK, apiResponse);
            //}
            //catch (LdapException ex)
            //{
            //    Logger.Log($"Error in DomainLogin: {ex.Message}");
            //    Logger.Log($"StackTrace in DomainLogin: {ex.StackTrace}");
            //    var status = HttpStatusCode.InternalServerError.ToString();
            //    var statusCode = (int)HttpStatusCode.OK;
            //    success = false;
            //    if (ex.Message == "The supplied credential is invalid.")
            //    {
            //        message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SYSTEM;
            //        error = Common.Constants.Errors.ERR_LOGIN_FAILED;
            //        status = HttpStatusCode.OK.ToString();
            //        statusCode = (int)HttpStatusCode.OK;
            //    }
            //    else
            //    {
            //        message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED;
            //        error = Common.Constants.Errors.ERR_INTERNAL_SERVER;
            //        status = HttpStatusCode.InternalServerError.ToString();
            //        statusCode = (int)HttpStatusCode.InternalServerError;
            //    }
            //    apiResponse = new ApiResponse<object>
            //    {
            //        Success = success,
            //        StatusCode = statusCode,
            //        Status = status,
            //        Message = message,
            //        Data = null,
            //        Error = error,
            //        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
            //    };
            //    return StatusCode(statusCode, apiResponse);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Log($"Error in Login: {ex.Message}");
            //    Logger.Log($"StackTrace in Login: {ex.StackTrace}");
            //    apiResponse = new ApiResponse<object>
            //    {
            //        Success = false,
            //        StatusCode = (int)HttpStatusCode.InternalServerError,
            //        Status = HttpStatusCode.InternalServerError.ToString(),
            //        Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
            //        Data = null,
            //        Error = Common.Constants.Errors.ERR_INTERNAL_SERVER,
            //        CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
            //    };
            //    return StatusCode(StatusCodes.Status500InternalServerError, apiResponse);
            //}
        }

        private bool IsADAvailable()
        {
            try
            {
                string ldapServer = _ldapSettings.Server; // ex: "ldap://192.168.1.10:389"

                using var connection = new LdapConnection(ldapServer);
                connection.Timeout = new TimeSpan(0, 0, 3); // 3 seconds timeout

                connection.Bind(); // simple ping bind

                return true; // AD reachable
            }
            catch
            {
                return false; // AD unavailable (network/DNS/server down)
            }
        }

        private bool TryADAuthentication(LoginRequest request)
        {
            try
            {
                string ldapServer = _ldapSettings.Server;

                var credential = new NetworkCredential(request.Username, request.Password);

                using var connection = new LdapConnection(ldapServer)
                {
                    Credential = credential,
                    AuthType = AuthType.Basic
                };

                connection.Bind(); // Will fail if wrong password

                return true;
            }
            catch
            {
                return false; // Authentication failed
            }
        }

    }
}