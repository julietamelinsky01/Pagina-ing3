using System.ComponentModel.DataAnnotations;

namespace LasMelis.Api.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El usuario es requerido.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
