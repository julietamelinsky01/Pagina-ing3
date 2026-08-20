using LasMelis.Api.Data;
using LasMelis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LasMelis.Api.Repositories;

public class AsignacionTurnoRepository : IAsignacionTurnoRepository
{
    private readonly AppDbContext _context;

    public AsignacionTurnoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AsignacionTurno>> GetByRangoAsync(DateOnly desde, DateOnly hasta) =>
        await _context.Asignaciones
            .Include(a => a.Empleado)
            .Include(a => a.TipoTurno)
            .Where(a => a.Fecha >= desde && a.Fecha <= hasta)
            .OrderBy(a => a.Fecha)
            .ThenBy(a => a.TipoTurno.HoraInicio)
            .ToListAsync();

    public async Task<AsignacionTurno?> GetByIdAsync(int id) =>
        await _context.Asignaciones
            .Include(a => a.Empleado)
            .Include(a => a.TipoTurno)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<AsignacionTurno?> GetDuplicadaAsync(int empleadoId, int tipoTurnoId, DateOnly fecha, int? excluirId = null)
    {
        var query = _context.Asignaciones.Where(a =>
            a.EmpleadoId == empleadoId && a.TipoTurnoId == tipoTurnoId && a.Fecha == fecha);

        if (excluirId.HasValue)
        {
            query = query.Where(a => a.Id != excluirId.Value);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<AsignacionTurno> AddAsync(AsignacionTurno asignacion)
    {
        _context.Asignaciones.Add(asignacion);
        await _context.SaveChangesAsync();
        return asignacion;
    }

    public async Task UpdateAsync(AsignacionTurno asignacion)
    {
        _context.Asignaciones.Update(asignacion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AsignacionTurno asignacion)
    {
        _context.Asignaciones.Remove(asignacion);
        await _context.SaveChangesAsync();
    }
}
