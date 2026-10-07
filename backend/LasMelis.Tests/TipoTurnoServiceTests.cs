using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;
using LasMelis.Api.Services;
using Moq;

namespace LasMelis.Tests;

public class TipoTurnoServiceTests
{
    private readonly Mock<ITipoTurnoRepository> _repo = new();
    private readonly TipoTurnoService _servicio;

    public TipoTurnoServiceTests()
    {
        _servicio = new TipoTurnoService(_repo.Object);
    }

    [Fact]
    public async Task Eliminar_UnTipoConAsignaciones_LanzaConflictoYNoBorra()
    {
        var tipo = new TipoTurno { Id = 1, Nombre = "Mañana" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tipo);
        _repo.Setup(r => r.TieneAsignacionesAsync(1)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictAppException>(() => _servicio.DeleteAsync(1));

        Assert.Contains("asignaciones", ex.Message);
        _repo.Verify(r => r.DeleteAsync(It.IsAny<TipoTurno>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_UnTipoSinAsignaciones_LoBorraUnaVez()
    {
        var tipo = new TipoTurno { Id = 1, Nombre = "Mañana" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tipo);
        _repo.Setup(r => r.TieneAsignacionesAsync(1)).ReturnsAsync(false);

        await _servicio.DeleteAsync(1);

        _repo.Verify(r => r.DeleteAsync(tipo), Times.Once);
    }

    [Fact]
    public async Task Eliminar_UnTipoInexistente_LanzaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TipoTurno?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.DeleteAsync(1));
    }

    [Fact]
    public async Task Crear_UnTipoNocturno_InformaSuDuracionDeOchoHoras()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<TipoTurno>())).ReturnsAsync((TipoTurno t) => t);

        var dto = await _servicio.CreateAsync(new TipoTurnoCreateDto
        {
            Nombre = "Noche", HoraInicio = new TimeOnly(22, 0), HoraFin = new TimeOnly(6, 0)
        });

        Assert.Equal(8, dto.HorasDuracion);
    }

    [Fact]
    public async Task Actualizar_UnTipoExistente_CambiaSusDatosYRecalculaLaDuracion()
    {
        var tipo = new TipoTurno { Id = 1, Nombre = "Mañana", HoraInicio = new TimeOnly(6, 0), HoraFin = new TimeOnly(14, 0) };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tipo);

        var dto = await _servicio.UpdateAsync(1, new TipoTurnoUpdateDto
        {
            Nombre = "Mañana larga", HoraInicio = new TimeOnly(6, 0), HoraFin = new TimeOnly(16, 0)
        });

        Assert.Equal("Mañana larga", tipo.Nombre);
        Assert.Equal(10, dto.HorasDuracion);
        _repo.Verify(r => r.UpdateAsync(tipo), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorId_Existente_DevuelveElTipoConSuDuracion()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
            new TipoTurno { Id = 1, Nombre = "Tarde", HoraInicio = new TimeOnly(14, 0), HoraFin = new TimeOnly(22, 0) });

        var dto = await _servicio.GetByIdAsync(1);

        Assert.Equal("Tarde", dto.Nombre);
        Assert.Equal(8, dto.HorasDuracion);
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_LanzaNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((TipoTurno?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.GetByIdAsync(5));
    }

    [Fact]
    public async Task ListarTodos_MapeaCadaTipoAdto()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TipoTurno>
        {
            new() { Id = 1, Nombre = "Mañana", HoraInicio = new TimeOnly(6, 0), HoraFin = new TimeOnly(14, 0) }
        });

        var lista = await _servicio.GetAllAsync();

        Assert.Single(lista);
        Assert.Equal(8, lista[0].HorasDuracion);
    }
}
