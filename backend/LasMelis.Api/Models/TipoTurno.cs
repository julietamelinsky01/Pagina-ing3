namespace LasMelis.Api.Models;

public class TipoTurno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    public ICollection<AsignacionTurno> Asignaciones { get; set; } = new List<AsignacionTurno>();
}
