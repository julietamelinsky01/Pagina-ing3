using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Models;
using LasMelis.Api.Repositories;

namespace LasMelis.Api.Services;

public class EmpleadoService : IEmpleadoService
{
    private readonly IEmpleadoRepository _repository;

    public EmpleadoService(IEmpleadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EmpleadoDto>> GetAllAsync(bool? activo)
    {
        var empleados = await _repository.GetAllAsync(activo);
        return empleados.Select(ToDto).ToList();
    }

    public async Task<EmpleadoDto> GetByIdAsync(int id)
    {
        var empleado = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró el empleado con id {id}.");
        return ToDto(empleado);
    }

    public async Task<EmpleadoDto> CreateAsync(EmpleadoCreateDto dto)
    {
        ValidarFechaIngreso(dto.FechaIngreso);

        var existente = await _repository.GetByDniAsync(dto.Dni);
        if (existente is not null)
        {
            throw new ConflictAppException($"Ya existe un empleado con el DNI {dto.Dni}.");
        }

        var empleado = new Empleado
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Dni = dto.Dni,
            Telefono = dto.Telefono,
            Email = dto.Email,
            FechaIngreso = dto.FechaIngreso,
            Activo = true
        };

        await _repository.AddAsync(empleado);
        return ToDto(empleado);
    }

    public async Task<EmpleadoDto> UpdateAsync(int id, EmpleadoUpdateDto dto)
    {
        var empleado = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró el empleado con id {id}.");

        ValidarFechaIngreso(dto.FechaIngreso);

        var existente = await _repository.GetByDniAsync(dto.Dni);
        if (existente is not null && existente.Id != id)
        {
            throw new ConflictAppException($"Ya existe un empleado con el DNI {dto.Dni}.");
        }

        empleado.Nombre = dto.Nombre;
        empleado.Apellido = dto.Apellido;
        empleado.Dni = dto.Dni;
        empleado.Telefono = dto.Telefono;
        empleado.Email = dto.Email;
        empleado.FechaIngreso = dto.FechaIngreso;

        await _repository.UpdateAsync(empleado);
        return ToDto(empleado);
    }

    public async Task<BajaEmpleadoResponseDto> BajaAsync(int id)
    {
        var empleado = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundAppException($"No se encontró el empleado con id {id}.");

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var asignacionesFuturas = await _repository.CountAsignacionesFuturasAsync(id, hoy);

        empleado.Activo = false;
        await _repository.UpdateAsync(empleado);

        var mensaje = asignacionesFuturas > 0
            ? $"Empleado dado de baja. Ojo: tiene {asignacionesFuturas} turno(s) asignado(s) a futuro que van a seguir apareciendo en el calendario."
            : "Empleado dado de baja correctamente.";

        return new BajaEmpleadoResponseDto
        {
            Empleado = ToDto(empleado),
            AsignacionesFuturasCount = asignacionesFuturas,
            Mensaje = mensaje
        };
    }

    private static void ValidarFechaIngreso(DateOnly fechaIngreso)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (fechaIngreso > hoy)
        {
            throw new ValidationAppException("La fecha de ingreso no puede ser futura.");
        }
    }

    private static EmpleadoDto ToDto(Empleado e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Apellido = e.Apellido,
        Dni = e.Dni,
        Telefono = e.Telefono,
        Email = e.Email,
        FechaIngreso = e.FechaIngreso,
        Activo = e.Activo
    };
}
