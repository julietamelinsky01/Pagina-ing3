using LasMelis.Api.Data;
using LasMelis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LasMelis.Api.Repositories;

public class TipoTurnoRepository : ITipoTurnoRepository
{
    private readonly AppDbContext _context;

    public TipoTurnoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoTurno>> GetAllAsync() =>
        await _context.TiposTurno.OrderBy(t => t.HoraInicio).ToListAsync();

    public async Task<TipoTurno?> GetByIdAsync(int id) =>
        await _context.TiposTurno.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TipoTurno> AddAsync(TipoTurno tipoTurno)
    {
        _context.TiposTurno.Add(tipoTurno);
        await _context.SaveChangesAsync();
        return tipoTurno;
    }

    public async Task UpdateAsync(TipoTurno tipoTurno)
    {
        _context.TiposTurno.Update(tipoTurno);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TipoTurno tipoTurno)
    {
        _context.TiposTurno.Remove(tipoTurno);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TieneAsignacionesAsync(int tipoTurnoId) =>
        await _context.Asignaciones.AnyAsync(a => a.TipoTurnoId == tipoTurnoId);
}
