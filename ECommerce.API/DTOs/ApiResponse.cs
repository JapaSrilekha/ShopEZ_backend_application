namespace ECommerce.API.DTOs
{

    // Generic API response wrapper.
    
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        // Creates a successful response with data and an optional message.
        public static ApiResponse<T> Ok(T data, string message = "Operation successful.")
            => new() { Success = true, Message = message, Data = data };

        
        // Creates a failure response with an error message and no data.
        
        public static ApiResponse<T> Fail(string message)
            => new() { Success = false, Message = message, Data = default };
    }
}
