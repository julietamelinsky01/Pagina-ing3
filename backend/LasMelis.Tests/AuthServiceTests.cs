using System.IdentityModel.Tokens.Jwt;
using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;
using LasMelis.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LasMelis.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();

    // IConfiguration real pero en memoria: no hace falta un doble, es un objeto simple.
    private static IConfiguration Config(string? key = "una-clave-secreta-de-prueba-de-al-menos-32-caracteres") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "LasMelisApi",
                ["Jwt:Audience"] = "LasMelisClient"
            })
            .Build();

    private AuthService Servicio(IConfiguration? config = null) => new(_usuarios.Object, config ?? Config());

    private static Usuario UsuarioConClave(string clave) => new()
    {
        Id = 1,
        Username = "admin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(clave)
    };

    [Fact]
    public async Task Login_ConUsuarioInexistente_LanzaNoAutorizado()
    {
        _usuarios.Setup(r => r.GetByUsernameAsync("nadie")).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            Servicio().LoginAsync(new LoginRequestDto { Username = "nadie", Password = "x" }));
    }

    [Fact]
    public async Task Login_ConContrasenaIncorrecta_LanzaNoAutorizadoConMensajeGenerico()
    {
        _usuarios.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(UsuarioConClave("correcta"));

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            Servicio().LoginAsync(new LoginRequestDto { Username = "admin", Password = "incorrecta" }));

        // El mensaje no revela si falló el usuario o la contraseña.
        Assert.Equal("Usuario o contraseña incorrectos.", ex.Message);
    }

    [Fact]
    public async Task Login_ConCredencialesCorrectas_DevuelveUnJwtFirmadoParaEseUsuario()
    {
        _usuarios.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(UsuarioConClave("correcta"));

        var respuesta = await Servicio().LoginAsync(new LoginRequestDto { Username = "admin", Password = "correcta" });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(respuesta.Token);
        Assert.Equal("admin", respuesta.Username);
        Assert.Equal("admin", token.Subject);
        Assert.Equal("LasMelisApi", token.Issuer);
        Assert.True(respuesta.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_SinClaveJwtConfigurada_FallaConUnErrorDeConfiguracion()
    {
        _usuarios.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(UsuarioConClave("correcta"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Servicio(Config(key: null)).LoginAsync(new LoginRequestDto { Username = "admin", Password = "correcta" }));

        Assert.Contains("Jwt:Key", ex.Message);
    }
}
