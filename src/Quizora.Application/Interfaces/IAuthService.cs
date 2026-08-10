using Quizora.Application.Common;
using Quizora.Application.DTOs.Auth;

namespace Quizora.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
}