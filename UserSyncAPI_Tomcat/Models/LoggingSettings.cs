namespace UserSyncAPI_Tomcat.Models
{
    public class LoggingSettings
    {
        public int LogFileRetentionDays { get; set; } = 7;
        public bool DeleteOldLogs { get; set; } = false;
    }
}
