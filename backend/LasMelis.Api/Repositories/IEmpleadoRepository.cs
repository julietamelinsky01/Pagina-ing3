using LasMelis.Api.Models;

namespace LasMelis.Api.Repositories;

public interface IEmpleadoRepository
{
    Task<List<Empleado>> GetAllAsync(bool? activo);
    Task<Empleado?> GetByIdAsync(int id);
    Task<Empleado?> GetByDniAsync(string dni);
    Task<Empleado> AddAsync(Empleado empleado);
    Task UpdateAsync(Empleado empleado);
    Task<int> CountAsignacionesFuturasAsync(int empleadoId, DateOnly desde);
}
