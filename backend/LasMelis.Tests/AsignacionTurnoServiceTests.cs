using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;
using LasMelis.Api.Services;
using Moq;

namespace LasMelis.Tests;

public class AsignacionTurnoServiceTests
{
    private readonly Mock<IAsignacionTurnoRepository> _asignaciones = new();
    private readonly Mock<IEmpleadoRepository> _empleados = new();
    private readonly Mock<ITipoTurnoRepository> _tiposTurno = new();
    private readonly AsignacionTurnoService _servicio;

    private static readonly DateOnly Fecha = new(2026, 9, 14);

    public AsignacionTurnoServiceTests()
    {
        _servicio = new AsignacionTurnoService(_asignaciones.Object, _empleados.Object, _tiposTurno.Object);
    }

    private static AsignacionTurnoCreateDto Dto() => new() { EmpleadoId = 1, TipoTurnoId = 2, Fecha = Fecha };

    private void EmpleadoExiste(bool activo) =>
        _empleados.Setup(r => r.GetByIdAsync(1))
                  .ReturnsAsync(new Empleado { Id = 1, Nombre = "Ana", Apellido = "Pérez", Activo = activo });

    private void TipoTurnoExiste(TimeOnly inicio, TimeOnly fin) =>
        _tiposTurno.Setup(r => r.GetByIdAsync(2))
                   .ReturnsAsync(new TipoTurno { Id = 2, Nombre = "Noche", HoraInicio = inicio, HoraFin = fin });

    [Fact]
    public async Task Crear_ParaUnEmpleadoInactivo_EsRechazadoYNoGuarda()
    {
        EmpleadoExiste(activo: false);

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() => _servicio.CreateAsync(Dto()));

        Assert.Contains("inactivo", ex.Message);
        _asignaciones.Verify(r => r.AddAsync(It.IsAny<AsignacionTurno>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConElEmpleadoInexistente_LanzaNotFound()
    {
        _empleados.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Empleado?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.CreateAsync(Dto()));
    }

    [Fact]
    public async Task Crear_ConElTipoDeTurnoInexistente_LanzaNotFound()
    {
        EmpleadoExiste(activo: true);
        _tiposTurno.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((TipoTurno?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.CreateAsync(Dto()));
    }

    [Fact]
    public async Task Crear_MismoEmpleadoTurnoYFecha_LanzaConflicto()
    {
        EmpleadoExiste(activo: true);
        TipoTurnoExiste(new TimeOnly(22, 0), new TimeOnly(6, 0));
        _asignaciones.Setup(r => r.GetDuplicadaAsync(1, 2, Fecha, null))
                     .ReturnsAsync(new AsignacionTurno { Id = 50 });

        await Assert.ThrowsAsync<ConflictAppException>(() => _servicio.CreateAsync(Dto()));
        _asignaciones.Verify(r => r.AddAsync(It.IsAny<AsignacionTurno>()), Times.Never);
    }

    [Fact]
    public async Task Crear_ConDatosValidos_GuardaYCalculaLasHorasDelTurnoNocturno()
    {
        EmpleadoExiste(activo: true);
        TipoTurnoExiste(new TimeOnly(22, 0), new TimeOnly(6, 0));
        _asignaciones.Setup(r => r.GetDuplicadaAsync(1, 2, Fecha, null)).ReturnsAsync((AsignacionTurno?)null);
        _asignaciones.Setup(r => r.AddAsync(It.IsAny<AsignacionTurno>())).ReturnsAsync((AsignacionTurno a) => a);

        var dto = await _servicio.CreateAsync(Dto());

        Assert.Equal(8, dto.HorasCalculadas);
        Assert.Equal("Ana Pérez", dto.EmpleadoNombreCompleto);
        _asignaciones.Verify(r => r.AddAsync(It.IsAny<AsignacionTurno>()), Times.Once);
    }

    [Fact]
    public async Task Actualizar_ExcluyeLaPropiaAsignacionAlBuscarDuplicadas()
    {
        // Si no se excluyera a sí misma, editar una asignación sin cambiar nada daría "duplicada".
        var existente = new AsignacionTurno { Id = 10, EmpleadoId = 1, TipoTurnoId = 2, Fecha = Fecha };
        _asignaciones.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existente);
        EmpleadoExiste(activo: true);
        TipoTurnoExiste(new TimeOnly(8, 0), new TimeOnly(16, 0));
        _asignaciones.Setup(r => r.GetDuplicadaAsync(1, 2, Fecha, 10)).ReturnsAsync((AsignacionTurno?)null);

        var dto = new AsignacionTurnoUpdateDto { EmpleadoId = 1, TipoTurnoId = 2, Fecha = Fecha, Observaciones = "cambio" };
        var resultado = await _servicio.UpdateAsync(10, dto);

        _asignaciones.Verify(r => r.GetDuplicadaAsync(1, 2, Fecha, 10), Times.Once);
        Assert.Equal("cambio", resultado.Observaciones);
    }

    [Fact]
    public async Task ListarPorRango_ConHastaAnteriorADesde_EsRechazado()
    {
        var desde = new DateOnly(2026, 9, 14);
        var hasta = new DateOnly(2026, 9, 13);   // borde: un día antes

        var ex = await Assert.ThrowsAsync<ValidationAppException>(() => _servicio.GetByRangoAsync(desde, hasta));

        Assert.Contains("anterior", ex.Message);
        _asignaciones.Verify(r => r.GetByRangoAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task ListarPorRango_ConRangoDeUnSoloDia_ConsultaElRepositorio()
    {
        // Borde opuesto: desde == hasta es válido.
        _asignaciones.Setup(r => r.GetByRangoAsync(Fecha, Fecha)).ReturnsAsync(new List<AsignacionTurno>());

        var lista = await _servicio.GetByRangoAsync(Fecha, Fecha);

        Assert.Empty(lista);
        _asignaciones.Verify(r => r.GetByRangoAsync(Fecha, Fecha), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorId_Existente_DevuelveLaAsignacionConSuTipoYEmpleado()
    {
        _asignaciones.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new AsignacionTurno
        {
            Id = 10, EmpleadoId = 1, TipoTurnoId = 2, Fecha = Fecha,
            Empleado = new Empleado { Nombre = "Ana", Apellido = "Pérez" },
            TipoTurno = new TipoTurno { Nombre = "Mañana", HoraInicio = new TimeOnly(6, 0), HoraFin = new TimeOnly(14, 0) }
        });

        var dto = await _servicio.GetByIdAsync(10);

        Assert.Equal("Mañana", dto.TipoTurnoNombre);
        Assert.Equal(8, dto.HorasCalculadas);
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_LanzaNotFound()
    {
        _asignaciones.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((AsignacionTurno?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.GetByIdAsync(10));
    }

    [Fact]
    public async Task Eliminar_UnaAsignacionInexistente_LanzaNotFound()
    {
        _asignaciones.Setup(r => r.GetByIdAsync(77)).ReturnsAsync((AsignacionTurno?)null);

        await Assert.ThrowsAsync<NotFoundAppException>(() => _servicio.DeleteAsync(77));
        _asignaciones.Verify(r => r.DeleteAsync(It.IsAny<AsignacionTurno>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_UnaAsignacionExistente_LaBorraUnaVez()
    {
        var existente = new AsignacionTurno { Id = 10 };
        _asignaciones.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existente);

        await _servicio.DeleteAsync(10);

        _asignaciones.Verify(r => r.DeleteAsync(existente), Times.Once);
    }
}
