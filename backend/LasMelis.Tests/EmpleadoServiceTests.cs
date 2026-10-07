using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;
using LasMelis.Api.Services;
using Moq;

namespace LasMelis.Tests;

// El repositorio (que habla con la base) se reemplaza por un doble de Moq:
// así probamos las REGLAS del servicio sin tocar PostgreSQL.
public class EmpleadoServiceTests
{
    private readonly Mock<IEmpleadoRepository> _repo = new();
    private readonly EmpleadoService _servicio;

    public EmpleadoServiceTests()
    {
        _servicio = new EmpleadoService(_repo.Object);
    }

    private static EmpleadoCreateDto DtoValido(string dni = "12345678") => new()
    {
        Nombre = "Ana",
        Apellido = "Pérez",
        Dni = dni,
        FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30)
    };

    [Fact]
    public async Task Crear_ConFechaDeIngresoFutura_EsRechazadoYNoGuarda()
    {
        var dto = DtoValido();
        dto.FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() => _servicio.CreateAsync(dto));

        Assert.Contains("futura", ex.Message);
        _repo.Verify(r => r.AddAsync(It.IsAny<Empleado>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConFechaDeIngresoDeHoy_EsValido()
    {
        // Borde exacto de la regla "no puede ser futura": hoy NO es futuro.
        var dto = DtoValido();
        dto.FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow);
        _repo.Setup(r => r.GetByDniAsync(It.IsAny<string>())).ReturnsAsync((Empleado?)null);
        _repo.Setup(r => r.AddAsync(It.IsAny<Empleado>())).ReturnsAsync((Empleado e) => e);

        var resultado = await _servicio.CreateAsync(dto);

        Assert.Equal(dto.FechaIngreso, resultado.FechaIngreso);
        _repo.Verify(r => r.AddAsync(It.IsAny<Empleado>()), Times.Once);
    }

    [Fact]
    public async Task Crear_ConDniYaExistente_LanzaConflictoQueNombraElDni()
    {
        _repo.Setup(r => r.GetByDniAsync("12345678"))
             .ReturnsAsync(new Empleado { Id = 7, Dni = "12345678" });

        var ex = await Assert.ThrowsAsync<ConflictAppException>(() => _servicio.CreateAsync(DtoValido("12345678")));

        Assert.Contains("12345678", ex.Message);
        _repo.Verify(r => r.AddAsync(It.IsAny<Empleado>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConDatosValidos_GuardaUnaSolaVezYQuedaActivo()
    {
        Empleado? guardado = null;
        _repo.Setup(r => r.GetByDniAsync(It.IsAny<string>())).ReturnsAsync((Empleado?)null);
        _repo.Setup(r => r.AddAsync(It.IsAny<Empleado>()))
             .Callback<Empleado>(e => guardado = e)
             .ReturnsAsync((Empleado e) => e);

        var resultado = await _servicio.CreateAsync(DtoValido());

        _repo.Verify(r => r.AddAsync(It.IsAny<Empleado>()), Times.Once);
        Assert.NotNull(guardado);
        Assert.True(guardado!.Activo);
        Assert.Equal("Ana", resultado.Nombre);
    }

    [Fact]
    public async Task Actualizar_ConservandoSuPropioDni_NoEsConflicto()
    {
        // Borde: el DNI "ya existe", pero es el del MISMO empleado que se edita.
        var empleado = new Empleado { Id = 5, Dni = "12345678", FechaIngreso = new DateOnly(2024, 1, 1) };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(empleado);
        _repo.Setup(r => r.GetByDniAsync("12345678")).ReturnsAsync(empleado);

        var dto = new EmpleadoUpdateDto
        {
            Nombre = "Ana", Apellido = "Gómez", Dni = "12345678",
            FechaIngreso = new DateOnly(2024, 1, 1)
        };

        var resultado = await _servicio.UpdateAsync(5, dto);

        Assert.Equal("Gómez", resultado.Apellido);
        _repo.Verify(r => r.UpdateAsync(empleado), Times.Once);
    }

    [Fact]
    public async Task Actualizar_CambiandoAUnDniLibre_GuardaElCambio()
    {
        // Camino feliz más común: el DNI nuevo no lo tiene nadie (GetByDniAsync devuelve null).
        var empleado = new Empleado { Id = 5, Dni = "11111111", FechaIngreso = new DateOnly(2024, 1, 1) };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(empleado);
        _repo.Setup(r => r.GetByDniAsync("33333333")).ReturnsAsync((Empleado?)null);

        var dto = new EmpleadoUpdateDto
        {
            Nombre = "Ana", Apellido = "Pérez", Dni = "33333333",
            FechaIngreso = new DateOnly(2024, 1, 1)
        };

        var resultado = await _servicio.UpdateAsync(5, dto);

        Assert.Equal("33333333", resultado.Dni);
        _repo.Verify(r => r.UpdateAsync(empleado), Times.Once);
    }

    [Fact]
    public async Task Actualizar_UnEmpleadoInexistente_LanzaNotFoundYNoGuarda()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Empleado?)null);
        var dto = new EmpleadoUpdateDto
        {
            Nombre = "Ana", Apellido = "Pérez", Dni = "12345678",
            FechaIngreso = new DateOnly(2024, 1, 1)
        };

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.UpdateAsync(99, dto));
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Empleado>()), Times.Never);
    }

    [Fact]
    public async Task Baja_DeUnEmpleadoInexistente_LanzaNotFoundYNoCuentaTurnos()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Empleado?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.BajaAsync(99));
        _repo.Verify(r => r.CountAsignacionesFuturasAsync(It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_ConElDniDeOtroEmpleado_LanzaConflicto()
    {
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Empleado { Id = 5, Dni = "11111111" });
        _repo.Setup(r => r.GetByDniAsync("22222222")).ReturnsAsync(new Empleado { Id = 9, Dni = "22222222" });

        var dto = new EmpleadoUpdateDto
        {
            Nombre = "Ana", Apellido = "Pérez", Dni = "22222222",
            FechaIngreso = new DateOnly(2024, 1, 1)
        };

        await Assert.ThrowsAsync<ConflictAppException>(() => _servicio.UpdateAsync(5, dto));
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Empleado>()), Times.Never);
    }

    [Fact]
    public async Task Baja_ConTurnosFuturos_AvisaCuantosSonYDesactiva()
    {
        var empleado = new Empleado { Id = 3, Activo = true };
        _repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(empleado);
        _repo.Setup(r => r.CountAsignacionesFuturasAsync(3, It.IsAny<DateOnly>())).ReturnsAsync(4);

        var respuesta = await _servicio.BajaAsync(3);

        Assert.False(empleado.Activo);
        Assert.Equal(4, respuesta.AsignacionesFuturasCount);
        Assert.Contains("4 turno(s)", respuesta.Mensaje);
        _repo.Verify(r => r.UpdateAsync(empleado), Times.Once);
    }

    [Fact]
    public async Task Baja_SinTurnosFuturos_DevuelveElMensajeSimple()
    {
        var empleado = new Empleado { Id = 3, Activo = true };
        _repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(empleado);
        _repo.Setup(r => r.CountAsignacionesFuturasAsync(3, It.IsAny<DateOnly>())).ReturnsAsync(0);

        var respuesta = await _servicio.BajaAsync(3);

        Assert.Equal("Empleado dado de baja correctamente.", respuesta.Mensaje);
        Assert.False(empleado.Activo);
    }

    [Fact]
    public async Task ObtenerPorId_Existente_DevuelveElEmpleadoMapeado()
    {
        _repo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(new Empleado { Id = 4, Nombre = "Lía", Apellido = "Ruiz", Dni = "30111222" });

        var dto = await _servicio.GetByIdAsync(4);

        Assert.Equal(4, dto.Id);
        Assert.Equal("Lía", dto.Nombre);
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_LanzaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Empleado?)null);

        var ex = await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.GetByIdAsync(99));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task ListarTodos_FiltraPorActivoYMapeaADto()
    {
        _repo.Setup(r => r.GetAllAsync(true)).ReturnsAsync(new List<Empleado>
        {
            new() { Id = 1, Nombre = "Ana", Apellido = "Pérez", Dni = "1", Activo = true }
        });

        var lista = await _servicio.GetAllAsync(true);

        Assert.Single(lista);
        Assert.Equal("Ana", lista[0].Nombre);
    }
}
