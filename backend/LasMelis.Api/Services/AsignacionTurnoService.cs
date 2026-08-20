using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;

namespace LasMelis.Api.Services;

public class AsignacionTurnoService : IAsignacionTurnoService
{
    private readonly IAsignacionTurnoRepository _repository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly ITipoTurnoRepository _tipoTurnoRepository;

    public AsignacionTurnoService(
        IAsignacionTurnoRepository repository,
        IEmpleadoRepository empleadoRepository,
        ITipoTurnoRepository tipoTurnoRepository)
    {
        _repository = repository;
        _empleadoRepository = empleadoRepository;
        _tipoTurnoRepository = tipoTurnoRepository;
    }

    public async Task<List<AsignacionTurnoDto>> GetByRangoAsync(DateOnly desde, DateOnly hasta)
    {
        if (hasta < desde)
        {
            throw new ValidationAppException("La fecha 'hasta' no puede ser anterior a la fecha 'desde'.");
        }

        var asignaciones = await _repository.GetByRangoAsync(desde, hasta);
        return asignaciones.Select(ToDto).ToList();
    }

    public async Task<AsignacionTurnoDto> GetByIdAsync(int id)
    {
        var asignacion = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró la asignación con id {id}.");
        return ToDto(asignacion);
    }

    public async Task<AsignacionTurnoDto> CreateAsync(AsignacionTurnoCreateDto dto)
    {
        var empleado = await ValidarEmpleadoActivoAsync(dto.EmpleadoId);
        var tipoTurno = await ValidarTipoTurnoExisteAsync(dto.TipoTurnoId);
        await ValidarNoDuplicadaAsync(dto.EmpleadoId, dto.TipoTurnoId, dto.Fecha);

        var asignacion = new AsignacionTurno
        {
            EmpleadoId = dto.EmpleadoId,
            TipoTurnoId = dto.TipoTurnoId,
            Fecha = dto.Fecha,
            Observaciones = dto.Observaciones
        };

        await _repository.AddAsync(asignacion);
        asignacion.Empleado = empleado;
        asignacion.TipoTurno = tipoTurno;
        return ToDto(asignacion);
    }

    public async Task<AsignacionTurnoDto> UpdateAsync(int id, AsignacionTurnoUpdateDto dto)
    {
        var asignacion = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró la asignación con id {id}.");

        var empleado = await ValidarEmpleadoActivoAsync(dto.EmpleadoId);
        var tipoTurno = await ValidarTipoTurnoExisteAsync(dto.TipoTurnoId);
        await ValidarNoDuplicadaAsync(dto.EmpleadoId, dto.TipoTurnoId, dto.Fecha, excluirId: id);

        asignacion.EmpleadoId = dto.EmpleadoId;
        asignacion.TipoTurnoId = dto.TipoTurnoId;
        asignacion.Fecha = dto.Fecha;
        asignacion.Observaciones = dto.Observaciones;

        await _repository.UpdateAsync(asignacion);
        asignacion.Empleado = empleado;
        asignacion.TipoTurno = tipoTurno;
        return ToDto(asignacion);
    }

    public async Task DeleteAsync(int id)
    {
        var asignacion = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró la asignación con id {id}.");

        await _repository.DeleteAsync(asignacion);
    }

    private async Task<Empleado> ValidarEmpleadoActivoAsync(int empleadoId)
    {
        var empleado = await _empleadoRepository.GetByIdAsync(empleadoId)
            ?? throw new NotFoundAppException($"No se encontró el empleado con id {empleadoId}.");

        if (!empleado.Activo)
        {
            throw new ValidationAppException(
                $"No se puede asignar un turno a {empleado.Nombre} {empleado.Apellido} porque está inactivo.");
        }

        return empleado;
    }

    private async Task<TipoTurno> ValidarTipoTurnoExisteAsync(int tipoTurnoId) =>
        await _tipoTurnoRepository.GetByIdAsync(tipoTurnoId)
            ?? throw new NotFoundAppException($"No se encontró el tipo de turno con id {tipoTurnoId}.");

    private async Task ValidarNoDuplicadaAsync(int empleadoId, int tipoTurnoId, DateOnly fecha, int? excluirId = null)
    {
        var duplicada = await _repository.GetDuplicadaAsync(empleadoId, tipoTurnoId, fecha, excluirId);
        if (duplicada is not null)
        {
            throw new ConflictAppException(
                "Ese empleado ya tiene asignado ese mismo turno en la fecha indicada.");
        }
    }

    private static AsignacionTurnoDto ToDto(AsignacionTurno a) => new()
    {
        Id = a.Id,
        EmpleadoId = a.EmpleadoId,
        EmpleadoNombreCompleto = $"{a.Empleado.Nombre} {a.Empleado.Apellido}",
        TipoTurnoId = a.TipoTurnoId,
        TipoTurnoNombre = a.TipoTurno.Nombre,
        HoraInicio = a.TipoTurno.HoraInicio,
        HoraFin = a.TipoTurno.HoraFin,
        HorasCalculadas = TurnoHorasCalculator.CalcularHoras(a.TipoTurno.HoraInicio, a.TipoTurno.HoraFin),
        Fecha = a.Fecha,
        Observaciones = a.Observaciones
    };
}
