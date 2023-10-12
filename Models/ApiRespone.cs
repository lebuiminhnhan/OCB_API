namespace OCB_API.Models
{
    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T Data { get; internal set; }
    }

}

