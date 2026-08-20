using LasMelis.Api.Models;

namespace LasMelis.Api.Repositories;

public interface ITipoTurnoRepository
{
    Task<List<TipoTurno>> GetAllAsync();
    Task<TipoTurno?> GetByIdAsync(int id);
    Task<TipoTurno> AddAsync(TipoTurno tipoTurno);
    Task UpdateAsync(TipoTurno tipoTurno);
    Task DeleteAsync(TipoTurno tipoTurno);
    Task<bool> TieneAsignacionesAsync(int tipoTurnoId);
}
