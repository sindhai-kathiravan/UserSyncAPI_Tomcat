namespace UserSyncAPI_Tomcat.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }  
        public string? Status { get; set; }
        public string? Message { get; set; }  
        public T? Data { get; set; }          
        public string? CorrelationId { get; set; }
        public string? Error { get; set; }
    }                                                                            
}