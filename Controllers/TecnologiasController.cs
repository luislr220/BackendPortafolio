using BackendPortafolio.Data;
using BackendPortafolio.DTOs;
using BackendPortafolio.Helpers;
using BackendPortafolio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendPortafolio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TecnologiasController : ControllerBase
{
    private readonly AppDbContext _context;
    public TecnologiasController(AppDbContext context)
    {
        _context = context;
    }

    //GET: api/tecnologias
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<TecnologiaReadDto>>>> GetTecnologias()
    {
        var resultado = await _context.Tecnologias.Select(t => new TecnologiaReadDto
        {
            Id = t.Id,
            Nombre = t.Nombre
        }).ToListAsync();

        return Ok(new ApiResponse<IEnumerable<TecnologiaReadDto>>
        {
            Exito = true,
            Mensaje = "Tecnologias obtenidas correctamente.",
            Datos = resultado
        });
    }

    //POST: api/tecnologias
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TecnologiaReadDto>>> PostTecnologia(TecnologiaDto tecnologiaDTO)
    {
        //Validar si ya existe para evitar duplicados
        if (await _context.Tecnologias.AnyAsync(t => t.Nombre.ToLower() == tecnologiaDTO.Nombre.ToLower()))
        {
            return BadRequest(new ApiResponse<TecnologiaReadDto> { Exito = false, Mensaje = "Esta tecnología ya existe." });
        }

        var tecnologia = new Tecnologia
        {
            Nombre = tecnologiaDTO.Nombre
        };

        _context.Tecnologias.Add(tecnologia);
        await _context.SaveChangesAsync();

        var resultado = new TecnologiaReadDto
        {
            Id = tecnologia.Id,
            Nombre = tecnologia.Nombre
        };

        var apiResponse = new ApiResponse<TecnologiaReadDto>
        {
            Exito = true,
            Mensaje = "Tecnología ingresada correctamente.",
            Datos = resultado
        };

        return CreatedAtAction(nameof(GetTecnologias), new { id = tecnologia.Id }, apiResponse);
    }

    //PUT: api/tecnologias/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> PutTecnologia(int id, TecnologiaDto tecnologiaDTO)
    {
        var tecnologiaEnDb = await _context.Tecnologias.FindAsync(id);
        if (tecnologiaEnDb == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "La tecnología no existe." });

        //VAlidar que el nuevo nombre no lo tenga otra tecnologia ya existente

        if (await _context.Tecnologias.AnyAsync(t => t.Nombre.ToLower() == tecnologiaDTO.Nombre.ToLower() && t.Id != id))
        {
            return BadRequest(new ApiResponse<string> { Exito = false, Mensaje = "Ya existe una tecnolgía con ese nombre." });
        }

        tecnologiaEnDb.Nombre = tecnologiaDTO.Nombre;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string> { Exito = true, Mensaje = "Tecnología actualizada correctamente." });
    }

    //DELETE: api/tecnologias/id
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTecnologia(int id)
    {
        var tecnologia = await _context.Tecnologias.FindAsync(id);
        if (tecnologia == null) return NotFound(new ApiResponse<string> { Exito = false, Mensaje = "La tenología no se encontro." });

        _context.Tecnologias.Remove(tecnologia);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Exito = true,
            Mensaje = $"La tecnología '{tecnologia.Nombre}' ha sido eliminada correctamente."
        });
    }
}