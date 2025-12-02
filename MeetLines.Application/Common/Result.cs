namespace MeetLines.Application.Common
{
    /// <summary>
    /// Clase genérica para manejar resultados de operaciones siguiendo el patrón Result.
    /// Implementa DDD hexagonal con separación de puertos y adaptadores.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public string? Error { get; private set; }
        public List<string>? Errors { get; private set; }

        // Propiedad de compatibilidad hacia atrás
        [Obsolete("Use IsSuccess instead")]
        public bool Success => IsSuccess;

        // Propiedad de compatibilidad hacia atrás
        [Obsolete("Use Value instead")]
        public T? Data => Value;

        // Propiedad de compatibilidad hacia atrás
        [Obsolete("Use Error instead")]
        public string? ErrorMessage => Error;

        private Result(bool isSuccess, T? value, string? error, List<string>? errors)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            Errors = errors;
        }

        public static Result<T> Ok(T value)
        {
            return new Result<T>(true, value, null, null);
        }

        public static Result<T> Fail(string error)
        {
            return new Result<T>(false, default, error, null);
        }

        public static Result<T> Fail(List<string> errors)
        {
            return new Result<T>(false, default, null, errors);
        }
    }

    /// <summary>
    /// Resultado sin datos (solo éxito/fallo) siguiendo el patrón Result.
    /// Implementa DDD hexagonal con separación de puertos y adaptadores.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public List<string>? Errors { get; private set; }

        // Propiedad de compatibilidad hacia atrás
        [Obsolete("Use IsSuccess instead")]
        public bool Success => IsSuccess;

        // Propiedad de compatibilidad hacia atrás
        [Obsolete("Use Error instead")]
        public string? ErrorMessage => Error;

        private Result(bool isSuccess, string? error, List<string>? errors)
        {
            IsSuccess = isSuccess;
            Error = error;
            Errors = errors;
        }

        public static Result Ok()
        {
            return new Result(true, null, null);
        }

        public static Result Fail(string error)
        {
            return new Result(false, error, null);
        }

        public static Result Fail(List<string> errors)
        {
            return new Result(false, null, errors);
        }
    }
}