using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;
using UserSyncAPI_Tomcat.Authentication;
using UserSyncAPI_Tomcat.Helpers;
using UserSyncAPI_Tomcat.Models;
using UserSyncAPI_Tomcat.Security;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            ApiResponse<User>? apiResponse = null;
            User? userObj = null;
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
                    string selectQuery = @"SELECT (SELECT STUFF((SELECT ',' + rtrim(FactoryCode)FROM UserFactoryMapping AS UFM WHERE UFM.UserId = U.user_id FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')) as FactoryList,
                       user_id, user_name, user_loginname, user_loggedonat, user_fullname, user_email,
                       user_initials, user_password, user_department, user_loggedin, user_inmodule,
                       g2version, lastlogin, os, clr, user_menus, logincount, libraries_readonly,
                       group_company, screen1_res, screen2_res, screen3_res, screen4_res,
                       po_auth_id, po_auth_all, order_alerts, po_auth_temp_user_id, maxordervalue,
                       outofoffice, default_order_department, porole_id, deleted, alias_username_1,
                       invoicebarcodeprinter, smtpserver, factory_id, piecemonitoringaccesslevel,
                       exclassedit, timesheetsaccesslevel, initial_windows, fabscheduleaccesslevel,
                       fablinescheduleaccesslevel, paintlinescheduleaccesslevel, contractsaccesslevel,
                       g2updaterversion, updatelocation_id, allocationadmin, user_password_last_changed,
                       date_created, createdbyuser_id, date_modified, modifiedbyuser_id,
                       loggedinoncomputer, yloc, ylocdsc, locked, fattempt, remarks, releasedt,
                       releaseby, inactive, inactiveremarks, inactivereleasedt, inactivereleaseby,
                       password_attempts--, password_updated_flag, unlock_date
                FROM Users U
                WHERE deleted = 0 and user_name = @UserName;";
                    string sql = string.Format(@"IF EXISTS (SELECT 1 FROM Users U    
                                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
                                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
                                    BEGIN
                                        UPDATE Users 
                                        SET user_loggedin = 1,fattempt=0, lastlogin = GETDATE() 
                                        WHERE user_name = @UserName;
                                        {0};
                                    END
                                    ELSE
                                    BEGIN
                                        UPDATE Users 
                                        SET fattempt = fattempt + 1 
                                        WHERE user_name = @UserName;
                                        SELECT * from Users WHERE 1 = 2;
                                    END", selectQuery);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", request.Username);
                        command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(request.Password));
                        command.Parameters.AddWithValue("@FactoryCode", request.SourceSystem);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userObj = new User
                                {
                                    UserId = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                    FactoryList = reader["FactoryList"] == DBNull.Value ? null : Convert.ToString(reader["FactoryList"]).Split(',').Select(f => f.Trim()).ToList(),
                                    UserName = reader["user_name"] != DBNull.Value ? Convert.ToString(reader["user_name"]) : null,
                                    UserLoginName = reader["user_loginname"] != DBNull.Value ? reader["user_loginname"].ToString() : null,
                                    UserLoggedOnAt = reader["user_loggedonat"] != DBNull.Value ? Convert.ToString(reader["user_loggedonat"]) : null,
                                    UserFullName = reader["user_fullname"] != DBNull.Value ? reader["user_fullname"].ToString() : null,
                                    UserEmail = reader["user_email"] != DBNull.Value ? reader["user_email"].ToString() : null,
                                    UserInitials = reader["user_initials"] != DBNull.Value ? reader["user_initials"].ToString() : null,
                                    //UserPassword = reader["user_password"] != DBNull.Value ? reader["user_password"].ToString() : null,
                                    UserDepartment = reader["user_department"] != DBNull.Value ? Convert.ToInt32(reader["user_department"]) : 0,
                                    UserLoggedIn = reader["user_loggedin"] == DBNull.Value ? false : Convert.ToBoolean(reader["user_loggedin"]),
                                    UserInModule = reader["user_inmodule"] != DBNull.Value ? Convert.ToInt32(reader["user_inmodule"]) : 0,
                                    G2Version = reader["g2version"] != DBNull.Value ? reader["g2version"].ToString() : null,
                                    LastLogin = reader["lastlogin"] != DBNull.Value ? Convert.ToDateTime(reader["lastlogin"]) : (DateTime?)null,
                                    OS = reader["os"] != DBNull.Value ? reader["os"].ToString() : null,
                                    CLR = reader["clr"] != DBNull.Value ? reader["clr"].ToString() : null,
                                    UserMenus = reader["user_menus"] != DBNull.Value ? reader["user_menus"].ToString() : null,
                                    LoginCount = reader["logincount"] != DBNull.Value ? Convert.ToInt32(reader["logincount"]) : 0,
                                    LibrariesReadOnly = reader["libraries_readonly"] != DBNull.Value ? Convert.ToBoolean(reader["libraries_readonly"]) : false,
                                    GroupCompany = reader["group_company"] != DBNull.Value ? Convert.ToInt32(reader["group_company"]) : 0,
                                    Screen1Res = reader["screen1_res"] != DBNull.Value ? reader["screen1_res"].ToString() : null,
                                    Screen2Res = reader["screen2_res"] != DBNull.Value ? reader["screen2_res"].ToString() : null,
                                    Screen3Res = reader["screen3_res"] != DBNull.Value ? reader["screen3_res"].ToString() : null,
                                    Screen4Res = reader["screen4_res"] != DBNull.Value ? reader["screen4_res"].ToString() : null,
                                    POAuthId = reader["po_auth_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_id"]) : 0,
                                    POAuthAll = reader["po_auth_all"] != DBNull.Value ? Convert.ToBoolean(reader["po_auth_all"]) : false,
                                    OrderAlerts = reader["order_alerts"] != DBNull.Value ? Convert.ToBoolean(reader["order_alerts"]) : false,
                                    POAuthTempUserId = reader["po_auth_temp_user_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_temp_user_id"]) : 0,
                                    MaxOrderValue = reader["maxordervalue"] != DBNull.Value ? Convert.ToInt32(reader["maxordervalue"]) : 0,
                                    OutOfOffice = reader["outofoffice"] != DBNull.Value ? Convert.ToBoolean(reader["outofoffice"]) : false,
                                    DefaultOrderDepartment = reader["default_order_department"] != DBNull.Value ? Convert.ToInt32(reader["default_order_department"]) : 0,
                                    PORoleId = reader["porole_id"] != DBNull.Value ? Convert.ToInt32(reader["porole_id"]) : 0,
                                    Deleted = reader["deleted"] != DBNull.Value ? Convert.ToBoolean(reader["deleted"]) : false,
                                    AliasUserName1 = reader["alias_username_1"] != DBNull.Value ? reader["alias_username_1"].ToString() : null,
                                    InvoiceBarcodePrinter = reader["invoicebarcodeprinter"] != DBNull.Value ? reader["invoicebarcodeprinter"].ToString() : null,
                                    SmtpServer = reader["smtpserver"] != DBNull.Value ? reader["smtpserver"].ToString() : null,
                                    FactoryId = reader["factory_id"] != DBNull.Value ? Convert.ToInt32(reader["factory_id"]) : 0,
                                    PieceMonitoringAccessLevel = reader["piecemonitoringaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["piecemonitoringaccesslevel"]) : 0,
                                    ExClassEdit = reader["exclassedit"] != DBNull.Value ? Convert.ToInt32(reader["exclassedit"]) : 0,
                                    TimesheetsAccessLevel = reader["timesheetsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["timesheetsaccesslevel"]) : 0,
                                    InitialWindows = reader["initial_windows"] != DBNull.Value ? reader["initial_windows"].ToString() : null,
                                    FabScheduleAccessLevel = reader["fabscheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fabscheduleaccesslevel"]) : 0,
                                    FabLineScheduleAccessLevel = reader["fablinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fablinescheduleaccesslevel"]) : 0,
                                    PaintLineScheduleAccessLevel = reader["paintlinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["paintlinescheduleaccesslevel"]) : 0,
                                    ContractsAccessLevel = reader["contractsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["contractsaccesslevel"]) : 0,
                                    G2UpdaterVersion = reader["g2updaterversion"] != DBNull.Value ? reader["g2updaterversion"].ToString() : null,
                                    UpdateLocationId = reader["updatelocation_id"] != DBNull.Value ? Convert.ToInt32(reader["updatelocation_id"]) : 0,
                                    AllocationAdmin = reader["allocationadmin"] != DBNull.Value ? Convert.ToBoolean(reader["allocationadmin"]) : false,
                                    UserPasswordLastChanged = reader["user_password_last_changed"] != DBNull.Value ? Convert.ToDateTime(reader["user_password_last_changed"]) : (DateTime?)null,
                                    DateCreated = reader["date_created"] != DBNull.Value ? Convert.ToDateTime(reader["date_created"]) : (DateTime?)null,
                                    CreatedByUserId = reader["createdbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["createdbyuser_id"]) : 0,
                                    DateModified = reader["date_modified"] != DBNull.Value ? Convert.ToDateTime(reader["date_modified"]) : (DateTime?)null,
                                    ModifiedByUserId = reader["modifiedbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["modifiedbyuser_id"]) : 0,
                                    LoggedInOnComputer = reader["loggedinoncomputer"] != DBNull.Value ? reader["loggedinoncomputer"].ToString() : null,
                                    YLoc = reader["yloc"] != DBNull.Value ? reader["yloc"].ToString() : null,
                                    YLocDsc = reader["ylocdsc"] != DBNull.Value ? reader["ylocdsc"].ToString() : null,
                                    Locked = reader["locked"] != DBNull.Value ? Convert.ToInt32(reader["locked"]) : 0,
                                    FAttempt = reader["fattempt"] != DBNull.Value ? Convert.ToInt32(reader["fattempt"]) : 0,
                                    Remarks = reader["remarks"] != DBNull.Value ? reader["remarks"].ToString() : null,
                                    ReleaseDt = reader["releasedt"] != DBNull.Value ? Convert.ToDateTime(reader["releasedt"]) : (DateTime?)null,
                                    ReleaseBy = reader["releaseby"] != DBNull.Value ? reader["releaseby"].ToString() : null,
                                    Inactive = reader["inactive"] != DBNull.Value ? Convert.ToInt32(reader["inactive"]) : 0,
                                    InactiveRemarks = reader["inactiveremarks"] != DBNull.Value ? reader["inactiveremarks"].ToString() : null,
                                    InactiveReleaseDt = reader["inactivereleasedt"] != DBNull.Value ? Convert.ToDateTime(reader["inactivereleasedt"]) : (DateTime?)null,
                                    InactiveReleaseBy = reader["inactivereleaseby"] != DBNull.Value ? reader["inactivereleaseby"].ToString() : null,
                                    PasswordAttempts = reader["password_attempts"] != DBNull.Value ? Convert.ToString(reader["password_attempts"]) : null//,
                                    //PasswordUpdatedFlag = reader["password_updated_flag"] != DBNull.Value ? Convert.ToString(reader["password_updated_flag"]) : null,
                                    //UnlockDate = reader["unlock_date"] != DBNull.Value ? Convert.ToDateTime(reader["unlock_date"]) : (DateTime?)null
                                };
                                success = true;
                                message = Common.Constants.Messages.USER_DATABASE_AUTHENTICATION_SUCCESSFUL;
                                error = null;
                            }
                            else
                            {
                                success = false;
                                message = ($"{Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SOURCE_SYSTEM} - {request.SourceSystem}");
                                error = Common.Constants.Errors.ERR_LOGIN_FAILED;
                            }
                        }
                    }
                }
                apiResponse = new ApiResponse<User>
                {
                    Success = success,
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = HttpStatusCode.OK.ToString(),
                    Message = message,
                    Data = userObj,
                    Error = error,
                    CorrelationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request)
                };
                return StatusCode(StatusCodes.Status200OK, apiResponse);

            }
            catch (Exception ex)
            {
                Logger.Log($"Error in Login: {ex.Message}");
                Logger.Log($"StackTrace in Login: {ex.StackTrace}");
                apiResponse = new ApiResponse<User>
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Status = HttpStatusCode.InternalServerError.ToString(),
                    Message = Common.Constants.Messages.AN_UNEXPECTED_ERROR_OCCURRED,
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
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request);
            ApiResponse<User>? apiResponse = null;
            string? error = null;
            Logger.Log($"{correlationId} > DomainLogin");

            bool adAvailable = IsADAvailable();

            if (adAvailable)
            {
                Logger.Log($"{correlationId} > AD Is Available");

                bool adSuccess = TryADAuthentication(request);

                if (adSuccess)
                {
                    Logger.Log($"{correlationId} > AD authentication successfull");

                    User? user = UpdatePasswordGetUser(request.Username, request.Password, request.SourceSystem);

                    apiResponse = new ApiResponse<User>
                    {
                        Success = true,
                        StatusCode = (int)HttpStatusCode.OK,
                        Status = HttpStatusCode.OK.ToString(),
                        Message = Common.Constants.Messages.USER_DOMAIN_AUTHENTICATION_SUCCESSFUL,
                        Data = user ,
                        Error = error,
                        CorrelationId = correlationId
                    };
                    return StatusCode(StatusCodes.Status200OK, apiResponse);
                }
                else
                {
                    Logger.Log($"{correlationId} > Credentials are invalid, AD authentication FAILURE");

                    // AD reachable but login failed → Fallback to DB
                    apiResponse = new ApiResponse<User>
                    {
                        Success = false,
                        StatusCode = (int)HttpStatusCode.OK,
                        Status = HttpStatusCode.OK.ToString(),
                        Message = Common.Constants.Messages.INVALID_USERNAME_OR_PASSWORD_FOR_THIS_DOMAIN,
                        Error = Common.Constants.Errors.ERR_LOGIN_FAILED,
                        CorrelationId = correlationId
                    };
                    return StatusCode(StatusCodes.Status200OK, apiResponse);
                }
            }
            else
            {
                Logger.Log($"{correlationId} > AD / Domain unavalable, trying for DB authentication.");

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

        private User? UpdatePasswordGetUser(string username, string password, string sourceSystem)
        {
            User? userObj = null;
            var config = new ConfigurationBuilder()
                                 .SetBasePath(AppContext.BaseDirectory)
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                 .Build();

            string? cnxnStr = config?.GetConnectionString(sourceSystem);

            using (SqlConnection connection = new SqlConnection(cnxnStr))
            {
                connection.Open();
                string selectQuery = @"SELECT (SELECT STUFF((SELECT ',' + rtrim(FactoryCode)FROM UserFactoryMapping AS UFM WHERE UFM.UserId = U.user_id FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')) as FactoryList,
                       user_id, user_name, user_loginname, user_loggedonat, user_fullname, user_email,
                       user_initials, user_password, user_department, user_loggedin, user_inmodule,
                       g2version, lastlogin, os, clr, user_menus, logincount, libraries_readonly,
                       group_company, screen1_res, screen2_res, screen3_res, screen4_res,
                       po_auth_id, po_auth_all, order_alerts, po_auth_temp_user_id, maxordervalue,
                       outofoffice, default_order_department, porole_id, deleted, alias_username_1,
                       invoicebarcodeprinter, smtpserver, factory_id, piecemonitoringaccesslevel,
                       exclassedit, timesheetsaccesslevel, initial_windows, fabscheduleaccesslevel,
                       fablinescheduleaccesslevel, paintlinescheduleaccesslevel, contractsaccesslevel,
                       g2updaterversion, updatelocation_id, allocationadmin, user_password_last_changed,
                       date_created, createdbyuser_id, date_modified, modifiedbyuser_id,
                       loggedinoncomputer, yloc, ylocdsc, locked, fattempt, remarks, releasedt,
                       releaseby, inactive, inactiveremarks, inactivereleasedt, inactivereleaseby,
                       password_attempts--, password_updated_flag, unlock_date
                FROM Users U
                WHERE deleted = 0 and user_name = @UserName;";
                string sql = string.Format(@"IF EXISTS (SELECT 1 FROM Users U    
                                            INNER JOIN UserFactoryMapping UF ON U.user_id = UF.UserId
                                            WHERE deleted = 0 AND U.user_name = @UserName AND U.user_password = @Password AND UF.FactoryCode = @FactoryCode)
                                    BEGIN
                                        UPDATE Users 
                                        SET user_password = @Password, user_loggedin = 1,fattempt=0, lastlogin = GETDATE() 
                                        WHERE user_name = @UserName;
                                        {0};
                                    END
                                    ELSE
                                    BEGIN
                                        UPDATE Users 
                                        SET fattempt = fattempt + 1 
                                        WHERE user_name = @UserName;
                                        SELECT * from Users WHERE 1 = 2;
                                    END", selectQuery);
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserName", username);
                    command.Parameters.AddWithValue("@Password", SecurityExtensions.Encrypt(password));
                    command.Parameters.AddWithValue("@FactoryCode", sourceSystem);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userObj = new User
                            {
                                UserId = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                FactoryList = reader["FactoryList"] == DBNull.Value ? null : Convert.ToString(reader["FactoryList"]).Split(',').Select(f => f.Trim()).ToList(),
                                UserName = reader["user_name"] != DBNull.Value ? Convert.ToString(reader["user_name"]) : null,
                                UserLoginName = reader["user_loginname"] != DBNull.Value ? reader["user_loginname"].ToString() : null,
                                UserLoggedOnAt = reader["user_loggedonat"] != DBNull.Value ? Convert.ToString(reader["user_loggedonat"]) : null,
                                UserFullName = reader["user_fullname"] != DBNull.Value ? reader["user_fullname"].ToString() : null,
                                UserEmail = reader["user_email"] != DBNull.Value ? reader["user_email"].ToString() : null,
                                UserInitials = reader["user_initials"] != DBNull.Value ? reader["user_initials"].ToString() : null,
                                //UserPassword = reader["user_password"] != DBNull.Value ? reader["user_password"].ToString() : null,
                                UserDepartment = reader["user_department"] != DBNull.Value ? Convert.ToInt32(reader["user_department"]) : 0,
                                UserLoggedIn = reader["user_loggedin"] == DBNull.Value ? false : Convert.ToBoolean(reader["user_loggedin"]),
                                UserInModule = reader["user_inmodule"] != DBNull.Value ? Convert.ToInt32(reader["user_inmodule"]) : 0,
                                G2Version = reader["g2version"] != DBNull.Value ? reader["g2version"].ToString() : null,
                                LastLogin = reader["lastlogin"] != DBNull.Value ? Convert.ToDateTime(reader["lastlogin"]) : (DateTime?)null,
                                OS = reader["os"] != DBNull.Value ? reader["os"].ToString() : null,
                                CLR = reader["clr"] != DBNull.Value ? reader["clr"].ToString() : null,
                                UserMenus = reader["user_menus"] != DBNull.Value ? reader["user_menus"].ToString() : null,
                                LoginCount = reader["logincount"] != DBNull.Value ? Convert.ToInt32(reader["logincount"]) : 0,
                                LibrariesReadOnly = reader["libraries_readonly"] != DBNull.Value ? Convert.ToBoolean(reader["libraries_readonly"]) : false,
                                GroupCompany = reader["group_company"] != DBNull.Value ? Convert.ToInt32(reader["group_company"]) : 0,
                                Screen1Res = reader["screen1_res"] != DBNull.Value ? reader["screen1_res"].ToString() : null,
                                Screen2Res = reader["screen2_res"] != DBNull.Value ? reader["screen2_res"].ToString() : null,
                                Screen3Res = reader["screen3_res"] != DBNull.Value ? reader["screen3_res"].ToString() : null,
                                Screen4Res = reader["screen4_res"] != DBNull.Value ? reader["screen4_res"].ToString() : null,
                                POAuthId = reader["po_auth_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_id"]) : 0,
                                POAuthAll = reader["po_auth_all"] != DBNull.Value ? Convert.ToBoolean(reader["po_auth_all"]) : false,
                                OrderAlerts = reader["order_alerts"] != DBNull.Value ? Convert.ToBoolean(reader["order_alerts"]) : false,
                                POAuthTempUserId = reader["po_auth_temp_user_id"] != DBNull.Value ? Convert.ToInt32(reader["po_auth_temp_user_id"]) : 0,
                                MaxOrderValue = reader["maxordervalue"] != DBNull.Value ? Convert.ToInt32(reader["maxordervalue"]) : 0,
                                OutOfOffice = reader["outofoffice"] != DBNull.Value ? Convert.ToBoolean(reader["outofoffice"]) : false,
                                DefaultOrderDepartment = reader["default_order_department"] != DBNull.Value ? Convert.ToInt32(reader["default_order_department"]) : 0,
                                PORoleId = reader["porole_id"] != DBNull.Value ? Convert.ToInt32(reader["porole_id"]) : 0,
                                Deleted = reader["deleted"] != DBNull.Value ? Convert.ToBoolean(reader["deleted"]) : false,
                                AliasUserName1 = reader["alias_username_1"] != DBNull.Value ? reader["alias_username_1"].ToString() : null,
                                InvoiceBarcodePrinter = reader["invoicebarcodeprinter"] != DBNull.Value ? reader["invoicebarcodeprinter"].ToString() : null,
                                SmtpServer = reader["smtpserver"] != DBNull.Value ? reader["smtpserver"].ToString() : null,
                                FactoryId = reader["factory_id"] != DBNull.Value ? Convert.ToInt32(reader["factory_id"]) : 0,
                                PieceMonitoringAccessLevel = reader["piecemonitoringaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["piecemonitoringaccesslevel"]) : 0,
                                ExClassEdit = reader["exclassedit"] != DBNull.Value ? Convert.ToInt32(reader["exclassedit"]) : 0,
                                TimesheetsAccessLevel = reader["timesheetsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["timesheetsaccesslevel"]) : 0,
                                InitialWindows = reader["initial_windows"] != DBNull.Value ? reader["initial_windows"].ToString() : null,
                                FabScheduleAccessLevel = reader["fabscheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fabscheduleaccesslevel"]) : 0,
                                FabLineScheduleAccessLevel = reader["fablinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["fablinescheduleaccesslevel"]) : 0,
                                PaintLineScheduleAccessLevel = reader["paintlinescheduleaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["paintlinescheduleaccesslevel"]) : 0,
                                ContractsAccessLevel = reader["contractsaccesslevel"] != DBNull.Value ? Convert.ToInt32(reader["contractsaccesslevel"]) : 0,
                                G2UpdaterVersion = reader["g2updaterversion"] != DBNull.Value ? reader["g2updaterversion"].ToString() : null,
                                UpdateLocationId = reader["updatelocation_id"] != DBNull.Value ? Convert.ToInt32(reader["updatelocation_id"]) : 0,
                                AllocationAdmin = reader["allocationadmin"] != DBNull.Value ? Convert.ToBoolean(reader["allocationadmin"]) : false,
                                UserPasswordLastChanged = reader["user_password_last_changed"] != DBNull.Value ? Convert.ToDateTime(reader["user_password_last_changed"]) : (DateTime?)null,
                                DateCreated = reader["date_created"] != DBNull.Value ? Convert.ToDateTime(reader["date_created"]) : (DateTime?)null,
                                CreatedByUserId = reader["createdbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["createdbyuser_id"]) : 0,
                                DateModified = reader["date_modified"] != DBNull.Value ? Convert.ToDateTime(reader["date_modified"]) : (DateTime?)null,
                                ModifiedByUserId = reader["modifiedbyuser_id"] != DBNull.Value ? Convert.ToInt32(reader["modifiedbyuser_id"]) : 0,
                                LoggedInOnComputer = reader["loggedinoncomputer"] != DBNull.Value ? reader["loggedinoncomputer"].ToString() : null,
                                YLoc = reader["yloc"] != DBNull.Value ? reader["yloc"].ToString() : null,
                                YLocDsc = reader["ylocdsc"] != DBNull.Value ? reader["ylocdsc"].ToString() : null,
                                Locked = reader["locked"] != DBNull.Value ? Convert.ToInt32(reader["locked"]) : 0,
                                FAttempt = reader["fattempt"] != DBNull.Value ? Convert.ToInt32(reader["fattempt"]) : 0,
                                Remarks = reader["remarks"] != DBNull.Value ? reader["remarks"].ToString() : null,
                                ReleaseDt = reader["releasedt"] != DBNull.Value ? Convert.ToDateTime(reader["releasedt"]) : (DateTime?)null,
                                ReleaseBy = reader["releaseby"] != DBNull.Value ? reader["releaseby"].ToString() : null,
                                Inactive = reader["inactive"] != DBNull.Value ? Convert.ToInt32(reader["inactive"]) : 0,
                                InactiveRemarks = reader["inactiveremarks"] != DBNull.Value ? reader["inactiveremarks"].ToString() : null,
                                InactiveReleaseDt = reader["inactivereleasedt"] != DBNull.Value ? Convert.ToDateTime(reader["inactivereleasedt"]) : (DateTime?)null,
                                InactiveReleaseBy = reader["inactivereleaseby"] != DBNull.Value ? reader["inactivereleaseby"].ToString() : null,
                                PasswordAttempts = reader["password_attempts"] != DBNull.Value ? Convert.ToString(reader["password_attempts"]) : null//,
                                                                                                                                                     //PasswordUpdatedFlag = reader["password_updated_flag"] != DBNull.Value ? Convert.ToString(reader["password_updated_flag"]) : null,
                                                                                                                                                     //UnlockDate = reader["unlock_date"] != DBNull.Value ? Convert.ToDateTime(reader["unlock_date"]) : (DateTime?)null
                            };
                        }
                        else
                        {
                            userObj = null;
                        }
                    }
                }
            }
            return userObj;
        }

        private bool IsADAvailable()
        {
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request);
            Logger.Log($"{correlationId} > Inside IsADAvailable function");

            try
            {
                string ldapServer = _ldapSettings.Server; // ex: "ldap://192.168.1.10:389"
                Logger.Log($"{correlationId} > LDAB Server domain {ldapServer}");

                using var connection = new LdapConnection(ldapServer);
                connection.Timeout = new TimeSpan(0, 0, 3); // 3 seconds timeout

                connection.Bind(); // simple ping bind
                Logger.Log($"{correlationId} > IsADAvailable LDAB Connection bind successfull");

                return true; // AD reachable
            }
            catch (Exception ex)
            {
                string errorDetails = ExceptionHelper.BuildExceptionDetails(ex);
                Logger.Log($"{correlationId} > IsADAvailable() FAILURE. \n\t {errorDetails}");
                return false; // AD unavailable (network/DNS/server down)
            }
        }

        //private bool TryADAuthentication(LoginRequest request)
        //{
        //    Logger.Log($"TryADAuthenticatione function");

        //    try
        //    {
        //        string ldapServer = _ldapSettings.Server;

        //        var credential = new NetworkCredential(request.Username, request.Password);

        //        using var connection = new LdapConnection(ldapServer)
        //        {
        //            Credential = credential,
        //            AuthType = AuthType.Basic
        //        };
        //        Logger.Log($"TryADAuthentication bind successfull");

        //        connection.Bind(); // Will fail if wrong password

        //        return true;
        //    }
        //    catch
        //    {
        //        Logger.Log($"TryADAuthentication failed");

        //        return false; // Authentication failed
        //    }
        //}

        private bool TryADAuthentication(LoginRequest request)
        {
            string? correlationId = CorrelationIdHelper.GetOrCreateCorrelationId(Request);
            Logger.Log($"{correlationId} > TryADAuthentication started");

            try
            {
                string ldapServer = _ldapSettings.Server;// "jssl.in";   // your domain controller DNS
                Logger.Log($"{correlationId} > ldapServer {ldapServer}");

                string userPrincipalName = $"{request.Username}@{ldapServer}";
                Logger.Log($"{correlationId} > userPrincipalName {userPrincipalName}");

                var credential = new NetworkCredential(userPrincipalName, request.Password);

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer))
                {
                    Credential = credential,
                    AuthType = AuthType.Negotiate   // secure authentication
                };

                // Optional: Skip certificate verification (development only)
                //    connection.SessionOptions.VerifyServerCertificate += (conn, cert) => true;

                // If password is wrong, this throws an exception
                connection.Bind();

                Logger.Log($"{correlationId} > TryADAuthentication successful");
                return true;
            }
            catch (Exception ex)
            {
                string errorDetails = ExceptionHelper.BuildExceptionDetails(ex);
                Logger.Log($"{correlationId} > TryADAuthentication() FAILURE. \n\t {errorDetails}");
                return false; // TryADAuthentication unavailable (network/DNS/server down)
            }
        }


    }
}