using LasMelis.Api.DTOs;
using LasMelis.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LasMelis.Api.Controllers;

[ApiController]
[Route("api/tipos-turno")]
[Authorize]
public class TiposTurnoController : ControllerBase
{
    private readonly ITipoTurnoService _service;

    public TiposTurnoController(ITipoTurnoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoTurnoDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TipoTurnoDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<TipoTurnoDto>> Create(TipoTurnoCreateDto dto)
    {
        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TipoTurnoDto>> Update(int id, TipoTurnoUpdateDto dto)
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
