using System.ComponentModel.DataAnnotations;

namespace LasMelis.Api.DTOs;

public class TipoTurnoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public double HorasDuracion { get; set; }
}

public class TipoTurnoCreateDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La hora de inicio es requerida.")]
    public TimeOnly HoraInicio { get; set; }

    [Required(ErrorMessage = "La hora de fin es requerida.")]
    public TimeOnly HoraFin { get; set; }
}

public class TipoTurnoUpdateDto : TipoTurnoCreateDto
{
}
