namespace MeetLines.API.DTOs
{
    /// <summary>
    /// Respuesta genérica de la API siguiendo el patrón de adaptador hexagonal.
    /// Este adaptador convierte los resultados internos a respuestas HTTP estandarizadas.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Operación exitosa"
            };
        }

        public static ApiResponse<T> Fail(string error, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = error,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Respuesta simple sin datos.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse Ok(string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message ?? "Operación exitosa"
            };
        }

        public static ApiResponse Fail(string error, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = error,
                Errors = errors
            };
        }
    }
}
