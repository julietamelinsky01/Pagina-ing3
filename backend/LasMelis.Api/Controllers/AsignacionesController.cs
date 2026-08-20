using LasMelis.Api.DTOs;
using LasMelis.Api.Exceptions;
using LasMelis.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LasMelis.Api.Controllers;

[ApiController]
[Route("api/asignaciones")]
[Authorize]
public class AsignacionesController : ControllerBase
{
    private readonly IAsignacionTurnoService _service;

    public AsignacionesController(IAsignacionTurnoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AsignacionTurnoDto>>> GetByRango(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        if (desde is null || hasta is null)
        {
            throw new ValidationAppException("Los parámetros 'desde' y 'hasta' son requeridos (formato YYYY-MM-DD).");
        }

        return Ok(await _service.GetByRangoAsync(desde.Value, hasta.Value));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AsignacionTurnoDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<AsignacionTurnoDto>> Create(AsignacionTurnoCreateDto dto)
    {
        var creada = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AsignacionTurnoDto>> Update(int id, AsignacionTurnoUpdateDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
