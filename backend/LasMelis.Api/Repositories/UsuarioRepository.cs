using LasMelis.Api.Data;
using LasMelis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LasMelis.Api.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByUsernameAsync(string username) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
}
