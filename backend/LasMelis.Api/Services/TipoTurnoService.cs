using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;

namespace LasMelis.Api.Services;

public class TipoTurnoService : ITipoTurnoService
{
    private readonly ITipoTurnoRepository _repository;

    public TipoTurnoService(ITipoTurnoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TipoTurnoDto>> GetAllAsync()
    {
        var tipos = await _repository.GetAllAsync();
        return tipos.Select(ToDto).ToList();
    }

    public async Task<TipoTurnoDto> GetByIdAsync(int id)
    {
        var tipo = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró el tipo de turno con id {id}.");
        return ToDto(tipo);
    }

    public async Task<TipoTurnoDto> CreateAsync(TipoTurnoCreateDto dto)
    {
        var tipo = new TipoTurno
        {
            Nombre = dto.Nombre,
            HoraInicio = dto.HoraInicio,
            HoraFin = dto.HoraFin
        };

        await _repository.AddAsync(tipo);
        return ToDto(tipo);
    }

    public async Task<TipoTurnoDto> UpdateAsync(int id, TipoTurnoUpdateDto dto)
    {
        var tipo = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró el tipo de turno con id {id}.");

        tipo.Nombre = dto.Nombre;
        tipo.HoraInicio = dto.HoraInicio;
        tipo.HoraFin = dto.HoraFin;

        await _repository.UpdateAsync(tipo);
        return ToDto(tipo);
    }

    public async Task DeleteAsync(int id)
    {
        var tipo = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró el tipo de turno con id {id}.");

        if (await _repository.TieneAsignacionesAsync(id))
        {
            throw new ConflictAppException(
                "No se puede eliminar este tipo de turno porque tiene asignaciones asociadas.");
        }

        await _repository.DeleteAsync(tipo);
    }

    private static TipoTurnoDto ToDto(TipoTurno t) => new()
    {
        Id = t.Id,
        Nombre = t.Nombre,
        HoraInicio = t.HoraInicio,
        HoraFin = t.HoraFin,
        HorasDuracion = TurnoHorasCalculator.CalcularHoras(t.HoraInicio, t.HoraFin)
    };
}
