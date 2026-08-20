using LasMelis.Api.Data;
using LasMelis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LasMelis.Api.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly AppDbContext _context;

    public EmpleadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Empleado>> GetAllAsync(bool? activo)
    {
        var query = _context.Empleados.AsQueryable();
        if (activo.HasValue)
        {
            query = query.Where(e => e.Activo == activo.Value);
        }
        return await query.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).ToListAsync();
    }

    public async Task<Empleado?> GetByIdAsync(int id) =>
        await _context.Empleados.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Empleado?> GetByDniAsync(string dni) =>
        await _context.Empleados.FirstOrDefaultAsync(e => e.Dni == dni);

    public async Task<Empleado> AddAsync(Empleado empleado)
    {
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
        return empleado;
    }

    public async Task UpdateAsync(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsignacionesFuturasAsync(int empleadoId, DateOnly desde) =>
        await _context.Asignaciones.CountAsync(a => a.EmpleadoId == empleadoId && a.Fecha >= desde);
}
