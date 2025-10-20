using Application.Interfaces;
using Common.Exceptions;
using Domain.Models.RequestModels;
using Domain.Models.RequestModels.AuthRequestModels;
using Domain.Models.ResponseModels.AuthResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza o login do usuário na aplicação.
        /// </summary>
        /// <param name="loginRequest">Credenciais de login (email e password).</param>
        /// <returns>Token JWT e informações do usuário autenticado.</returns>
        /// <response code="200">Login realizado com sucesso.</response>
        /// <response code="401">Credenciais inválidas ou usuário não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseModel), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginRequestModel loginRequest)
        {
            try
            {
                var result = await _authService.LoginAsync(loginRequest);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Registra um novo usuário na aplicação.
        /// </summary>
        /// <param name="registerRequest">Dados do novo usuário para registro.</param>
        /// <returns>Token JWT e informações do usuário registrado.</returns>
        /// <response code="200">Usuário registrado com sucesso.</response>
        /// <response code="400">Dados inválidos ou email/username já cadastrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponseModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RegisterResponseModel>> Register([FromBody] RegisterRequestModel registerRequest)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerRequest);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Obtém as informações do usuário atualmente autenticado.
        /// </summary>
        /// <returns>Informações completas do usuário e cliente associado.</returns>
        /// <response code="200">Informações do usuário retornadas com sucesso.</response>
        /// <response code="401">Token inválido ou usuário não autenticado.</response>
        /// <response code="404">Usuário não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("me")]
        [Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(typeof(UserProfileResponseModel), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserProfileResponseModel>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _authService.GetUserProfileAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Atualiza os dados do perfil do cliente. 
        /// Sem clientId: atualiza próprio perfil. Com clientId: administrador atualiza qualquer cliente.
        /// </summary>
        /// <param name="clientId">ID opcional do cliente (apenas para administradores).</param>
        /// <param name="request">Dados atualizados do cliente.</param>
        /// <returns>Nenhum conteúdo se a atualização for bem-sucedida.</returns>
        /// <response code="204">Perfil atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="401">Usuário não autenticado.</response>
        /// <response code="403">Usuário não tem permissão para atualizar outros clientes.</response>
        /// <response code="404">Usuário ou cliente não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPut("profile")]
        [Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] ClientRequestModel request, [FromQuery] int? clientId = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _authService.UpdateUserProfileAsync(userId, clientId, request);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ValidationErrors);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Exclui a conta do usuário. 
        /// Sem clientId: usuário exclui própria conta. Com clientId: administrador exclui qualquer conta.
        /// </summary>
        /// <param name="clientId">ID opcional do cliente (apenas para administradores).</param>
        /// <returns>Nenhum conteúdo se a exclusão for bem-sucedida.</returns>
        /// <response code="204">Conta excluída com sucesso.</response>
        /// <response code="401">Usuário não autenticado.</response>
        /// <response code="403">Usuário não tem permissão para excluir outras contas.</response>
        /// <response code="404">Usuário ou cliente não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpDelete("account")]
        [Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteUserProfile([FromQuery] int? clientId = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _authService.DeleteUserProfileAsync(userId, clientId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
