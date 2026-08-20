using System.ComponentModel.DataAnnotations;

namespace LasMelis.Api.DTOs;

public class EmpleadoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public bool Activo { get; set; }
}

public class EmpleadoCreateDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es requerido.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe ser numérico, de 7 u 8 dígitos.")]
    public string Dni { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "El email no tiene un formato válido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es requerida.")]
    public DateOnly FechaIngreso { get; set; }
}

public class EmpleadoUpdateDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es requerido.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe ser numérico, de 7 u 8 dígitos.")]
    public string Dni { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "El email no tiene un formato válido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es requerida.")]
    public DateOnly FechaIngreso { get; set; }
}

public class BajaEmpleadoResponseDto
{
    public EmpleadoDto Empleado { get; set; } = null!;
    public int AsignacionesFuturasCount { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
