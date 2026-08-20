using LasMelis.Api.Models;

namespace LasMelis.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByUsernameAsync(string username);
}
