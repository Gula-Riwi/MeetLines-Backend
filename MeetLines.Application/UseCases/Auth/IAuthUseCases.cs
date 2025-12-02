using MeetLines.Application.Common;
using MeetLines.Application.DTOs.Auth;

namespace MeetLines.Application.UseCases.Auth
{
    /// <summary>
    /// Puerto de entrada (Use Case) para el registro de usuarios.
    /// Implementa DDD hexagonal: define el contrato del caso de uso sin detalles de implementación.
    /// </summary>
    public interface IRegisterUserUseCase
    {
        Task<Result<RegisterResponse>> ExecuteAsync(RegisterRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para el login de usuarios.
    /// </summary>
    public interface ILoginUserUseCase
    {
        Task<Result<LoginResponse>> ExecuteAsync(LoginRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para OAuth login.
    /// </summary>
    public interface IOAuthLoginUseCase
    {
        Task<Result<LoginResponse>> ExecuteAsync(OAuthLoginRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para refrescar token.
    /// </summary>
    public interface IRefreshTokenUseCase
    {
        Task<Result<LoginResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para verificar email.
    /// </summary>
    public interface IVerifyEmailUseCase
    {
        Task<Result> ExecuteAsync(VerifyEmailRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para solicitar recuperación de contraseña.
    /// </summary>
    public interface IForgotPasswordUseCase
    {
        Task<Result> ExecuteAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para restablecer contraseña.
    /// </summary>
    public interface IResetPasswordUseCase
    {
        Task<Result> ExecuteAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para reenviar email de verificación.
    /// </summary>
    public interface IResendVerificationEmailUseCase
    {
        Task<Result> ExecuteAsync(string email, CancellationToken ct = default);
    }

    /// <summary>
    /// Puerto de entrada (Use Case) para logout.
    /// </summary>
    public interface ILogoutUseCase
    {
        Task<Result> ExecuteAsync(string refreshToken, CancellationToken ct = default);
    }
}
