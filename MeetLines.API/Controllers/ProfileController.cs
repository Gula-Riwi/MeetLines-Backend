using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetLines.Application.DTOs.Profile;
using MeetLines.Application.Services.Interfaces;
using MeetLines.API.DTOs;

namespace MeetLines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// GET: api/profile
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<GetProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse.Fail("Token inválido"));

            var result = await _profileService.GetProfileAsync(userId, ct);

            // CORREGIDO: Success -> IsSuccess, ErrorMessage -> Error
            if (!result.IsSuccess)
                return NotFound(ApiResponse<GetProfileResponse>.Fail(result.Error ?? "Perfil no encontrado"));

            // CORREGIDO: Data -> Value
            return Ok(ApiResponse<GetProfileResponse>.Ok(result.Value!));
        }

        /// <summary>
        /// Actualiza el perfil del usuario autenticado
        /// PUT: api/profile
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<GetProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse.Fail("Token inválido"));

            var result = await _profileService.UpdateProfileAsync(userId, request, ct);

            // CORREGIDO: Success -> IsSuccess, ErrorMessage -> Error
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<GetProfileResponse>.Fail(result.Error ?? "Error al actualizar perfil"));

            // CORREGIDO: Data -> Value
            return Ok(ApiResponse<GetProfileResponse>.Ok(result.Value!, "Perfil actualizado exitosamente"));
        }

        /// <summary>
        /// Actualiza la foto de perfil del usuario autenticado
        /// PUT: api/profile/picture
        /// </summary>
        [HttpPut("picture")]
        [ProducesResponseType(typeof(ApiResponse<GetProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse.Fail("Token inválido"));

            var result = await _profileService.UpdateProfilePictureAsync(userId, request, ct);

            // CORREGIDO: Success -> IsSuccess, ErrorMessage -> Error
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<GetProfileResponse>.Fail(result.Error ?? "Error al actualizar foto"));

            // CORREGIDO: Data -> Value
            return Ok(ApiResponse<GetProfileResponse>.Ok(result.Value!, "Foto de perfil actualizada exitosamente"));
        }

        /// <summary>
        /// Elimina la foto de perfil del usuario autenticado
        /// DELETE: api/profile/picture
        /// </summary>
        [HttpDelete("picture")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> DeleteProfilePicture(CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse.Fail("Token inválido"));

            var result = await _profileService.DeleteProfilePictureAsync(userId, ct);

            // CORREGIDO: Success -> IsSuccess, ErrorMessage -> Error
            if (!result.IsSuccess)
                return BadRequest(ApiResponse.Fail(result.Error ?? "Error al eliminar foto"));

            return Ok(ApiResponse.Ok("Foto de perfil eliminada exitosamente"));
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// PUT: api/profile/change-password
        /// </summary>
        [HttpPut("change-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            if (userId == Guid.Empty)
                return Unauthorized(ApiResponse.Fail("Token inválido"));

            var result = await _profileService.ChangePasswordAsync(userId, request, ct);

            // CORREGIDO: Success -> IsSuccess, ErrorMessage -> Error
            if (!result.IsSuccess)
                return BadRequest(ApiResponse.Fail(result.Error ?? "Error al cambiar contraseña"));

            return Ok(ApiResponse.Ok("Contraseña cambiada exitosamente. Todas tus sesiones han sido cerradas por seguridad."));
        }

        // Helper method para extraer el UserId del token JWT
        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }
            return userId;
        }
    }
}