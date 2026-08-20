using LasMelis.Api.DTOs;
using LasMelis.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LasMelis.Api.Controllers;

[ApiController]
[Route("api/empleados")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoService _service;

    public EmpleadosController(IEmpleadoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmpleadoDto>>> GetAll([FromQuery] bool? activo)
    {
        return Ok(await _service.GetAllAsync(activo));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmpleadoDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<EmpleadoDto>> Create(EmpleadoCreateDto dto)
    {
        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmpleadoDto>> Update(int id, EmpleadoUpdateDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BajaEmpleadoResponseDto>> Baja(int id)
    {
        return Ok(await _service.BajaAsync(id));
    }
}
