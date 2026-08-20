using LasMelis.Api.DTOs;

namespace LasMelis.Api.Services;

public interface IAsignacionTurnoService
{
    Task<List<AsignacionTurnoDto>> GetByRangoAsync(DateOnly desde, DateOnly hasta);
    Task<AsignacionTurnoDto> GetByIdAsync(int id);
    Task<AsignacionTurnoDto> CreateAsync(AsignacionTurnoCreateDto dto);
    Task<AsignacionTurnoDto> UpdateAsync(int id, AsignacionTurnoUpdateDto dto);
    Task DeleteAsync(int id);
}
