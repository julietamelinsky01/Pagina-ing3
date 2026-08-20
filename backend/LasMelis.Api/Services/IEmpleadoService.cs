using LasMelis.Api.DTOs;

namespace LasMelis.Api.Services;

public interface IEmpleadoService
{
    Task<List<EmpleadoDto>> GetAllAsync(bool? activo);
    Task<EmpleadoDto> GetByIdAsync(int id);
    Task<EmpleadoDto> CreateAsync(EmpleadoCreateDto dto);
    Task<EmpleadoDto> UpdateAsync(int id, EmpleadoUpdateDto dto);
    Task<BajaEmpleadoResponseDto> BajaAsync(int id);
}
