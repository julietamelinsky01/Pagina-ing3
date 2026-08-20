using LasMelis.Api.DTOs;

namespace LasMelis.Api.Services;

public interface ITipoTurnoService
{
    Task<List<TipoTurnoDto>> GetAllAsync();
    Task<TipoTurnoDto> GetByIdAsync(int id);
    Task<TipoTurnoDto> CreateAsync(TipoTurnoCreateDto dto);
    Task<TipoTurnoDto> UpdateAsync(int id, TipoTurnoUpdateDto dto);
    Task DeleteAsync(int id);
}
