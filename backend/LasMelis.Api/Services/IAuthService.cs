using LasMelis.Api.DTOs;

namespace LasMelis.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
}
