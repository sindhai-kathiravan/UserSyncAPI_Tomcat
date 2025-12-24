    namespace UserSyncAPI_Tomcat.Models
{
    public class UserResponseData
    {
        public User? User { get; set; }

    }

    public class EmptyData
    {
    }
    public class ValidationMessagesData
    {
        public List<string> ValidationMessages { get; set; }
    }
    public class ValidationResponseData
    {
        public string? ValidationMessage { get; set; }
    }
    public class ValidationErrorData
    {
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
