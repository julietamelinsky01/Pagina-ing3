using LasMelis.Api.Models;

namespace LasMelis.Api.Repositories;

public interface IAsignacionTurnoRepository
{
    Task<List<AsignacionTurno>> GetByRangoAsync(DateOnly desde, DateOnly hasta);
    Task<AsignacionTurno?> GetByIdAsync(int id);
    Task<AsignacionTurno?> GetDuplicadaAsync(int empleadoId, int tipoTurnoId, DateOnly fecha, int? excluirId = null);
    Task<AsignacionTurno> AddAsync(AsignacionTurno asignacion);
    Task UpdateAsync(AsignacionTurno asignacion);
    Task DeleteAsync(AsignacionTurno asignacion);
}
