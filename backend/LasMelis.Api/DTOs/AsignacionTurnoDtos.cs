using System.ComponentModel.DataAnnotations;

namespace LasMelis.Api.DTOs;

public class AsignacionTurnoDto
{
    public int Id { get; set; }
    public int EmpleadoId { get; set; }
    public string EmpleadoNombreCompleto { get; set; } = string.Empty;
    public int TipoTurnoId { get; set; }
    public string TipoTurnoNombre { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public double HorasCalculadas { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Observaciones { get; set; }
}

public class AsignacionTurnoCreateDto
{
    [Required(ErrorMessage = "El empleado es requerido.")]
    public int EmpleadoId { get; set; }

    [Required(ErrorMessage = "El tipo de turno es requerido.")]
    public int TipoTurnoId { get; set; }

    [Required(ErrorMessage = "La fecha es requerida.")]
    public DateOnly Fecha { get; set; }

    public string? Observaciones { get; set; }
}

public class AsignacionTurnoUpdateDto : AsignacionTurnoCreateDto
{
}
