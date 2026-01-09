namespace UserSyncAPI_Tomcat.Common
{
    public static class Constants
    {
        public static class Headers
        {
            public const string CORRELATION_ID = "X-Correlation-Id";
        }
        public static class Messages
        {
            public const string REQUEST_BODY_IS_NULL = "Request body is null.";
            public const string USER_RETRIEVED_SUCCESSFULLY = "User retrieved successfully.";
            public const string REQUEST_COMPLETED_SUCCESSFULLY = "Request completed successfully.";
            public const string INVALID_USER_ID = "Invalid user Id.";
            public const string THE_REQUESTED_USER_WAS_NOT_FOUND = "The requested user was not found.";
            public const string THE_REQUEST_IS_INVALID = "The request is invalid.";
            public const string AUTHORIZATION_HAS_BEEN_DENIED_FOR_THIS_REQUEST = "Authorization has been denied for this request.";
            public const string INVALID_API_CREDENTIALS = "Invalid API credentials. Please check your API key or Basic Auth header.";
            public const string USER_CREATED_SUCCESSFULLY = "User created successfully.";
            public const string AN_UNEXPECTED_ERROR_OCCURRED = "An unexpected error occurred.";
            public const string AT_LEAST_ONE_TARGET_DATABASE_MUST_BE_SPECIFIED = "At least one target database must be specified.";
            public const string INVALID_DATABASE_KEY_FOUND = "Invalid database key found in TargetFactories.";
            public const string USER_UPDATED_SUCCESSFULLY = "User updated successfully.";
            public const string USER_ID_DOES_NOT_EXIST = "UserId does not exist.";
            public const string INVALID_SOURCE_SYSTEM = "Invalid Source System.";
            public const string THE_SOURCE_SYSTEM_XXXX_DOES_NOT_EXIST_IN_THE_SYSTEM_LIST = "The source system '{0}' does not exist in the system list.";
            public const string THE_USER_ID_XX_IS_INVALID = "The user id '{0}' is invalid.";
            public const string USERID_QUERY_PARAMETER_IS_REQUIRED = "UserId query parameter 'id' is required.";
            public const string USER_DELETED_SUCCESSFULLY = "User deleted successfully.";
            public const string USER_DATABASE_AUTHENTICATION_SUCCESSFUL = "User Database Authentication successful.";
            public const string USER_DOMAIN_AUTHENTICATION_SUCCESSFUL = "User Domain Authentication successful.";
            public const string INVALID_USERNAME_OR_PASSWORD_FOR_THIS_DOMAIN = "Invalid username or password for this Domain.";
            public const string INVALID_USERNAME_OR_PASSWORD_FOR_THIS_SOURCE_SYSTEM = "Invalid username or password for this Source System";
            public const string USERNAME_ALREADY_EXIST = "Username already exist.";
            public const string MAX_IDS_ARE_NOT_THE_SAME_ACROSS_DATABASES = "Max Ids are not the same across databases.";
        }
        public static class AuthenticationSchemes { 
            public const string BasicAuthentication = "BasicAuthentication";

        }
        public static class ActionNames
        {
            public const string CreateUser = "Create";
            public const string GetUser = "Get";
            public const string GetAllUser = "GetAll";
            public const string UpdateUser = "Update";
            public const string DeleteUser = "Delete";
            public const string Login = "Login";
            public const string DomainLogin = "DomainLogin";
        }
        public static class Methods
        {
            public const string GET = "GET";
            public const string POST = "POST";
            public const string PUT = "PUT";
            public const string PATCH = "PATCH";
            public const string DELETE = "DELETE";
        }
        public static class QueryStrings
        {
            public const string UserId = "Id";
            public const string SourceSystem = "SourceSystem";
        }
        public static class ConfigKeys
        {
            public const string API_USERNAME = "API_UserName";
            public const string API_PASSWORD = "API_Password";
            public const string LOG_FILE_RETENTION_DAYS = "LogFileRetentionDays";
            public const string DELETE_OLD_LOGS = "DeleteOldLogs";
            public const string LOCAL_SQLSERVER = "LocalSqlServer";
        }
        public static class Errors
        {
            public const string ERR_NULL_BODY = "ERR_NULL_BODY";
            public const string ERR_INTERNAL_SERVER = "ERR_INTERNAL_SERVER";
            public const string ERR_NOT_FOUND = "ERR_NOT_FOUND";
            public const string ERR_BAD_REQUEST = "ERR_BAD_REQUEST";
            public const string ERR_UNAUTHORIZED = "ERR_UNAUTHORIZED";
            public const string ERR_VALIDATION_FAILUED = "ERR_VALIDATION_FAILUER";
            public const string ERR_LOGIN_FAILED = "ERR_LOGIN_FAILED";
        }
    }
}