using System.Collections.Generic;

namespace TutorPlatform.API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public List<string> Messages { get; set; } = new List<string>();

        public static ApiResponse<T> Ok(T data)
        {
            return new ApiResponse<T> { Success = true, StatusCode = 200, Data = data };
        }
        
        public static ApiResponse<T> Created(T data)
        {
            return new ApiResponse<T> { Success = true, StatusCode = 201, Data = data };
        }

        public static ApiResponse<T> Error(int statusCode, params string[] messages)
        {
            return new ApiResponse<T> { Success = false, StatusCode = statusCode, Messages = new List<string>(messages) };
        }
    }
}
