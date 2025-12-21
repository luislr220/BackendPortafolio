using System.Collections;
using BackendPortafolio.Data;
using BackendPortafolio.DTOs;
using BackendPortafolio.Models;
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
    public async Task<ActionResult<IEnumerable<TecnologiaReadDto>>> GetTecnologias()
    {
        return await _context.Tecnologias.Select(t => new TecnologiaReadDto
        {
            Id = t.Id,
            Nombre = t.Nombre
        }).ToListAsync();
    }

    //POST: api/tecnologias
    [HttpPost]
    public async Task<ActionResult<TecnologiaReadDto>> PostTecnologia(TecnologiaDTO tecnologiaDTO)
    {
        //Validar si ya existe para evitar duplicados
        if (await _context.Tecnologias.AnyAsync(t => t.Nombre.ToLower() == tecnologiaDTO.Nombre.ToLower()))
        {
            return BadRequest("Esta tecnología ya existe");
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

        return CreatedAtAction(nameof(GetTecnologias), new { id = tecnologia.Id }, resultado);
    }

    //PUT: api/tecnologias/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTecnologia(int id, TecnologiaDTO tecnologiaDTO)
    {
        var tecnologiaEnDb = await _context.Tecnologias.FindAsync(id);
        if (tecnologiaEnDb == null) return NotFound("La tecnología no existe");

        //VAlidar que el nuevo nombre no lo tenga otra tecnologia ya existente

        if (await _context.Tecnologias.AnyAsync(t => t.Nombre.ToLower() == tecnologiaDTO.Nombre.ToLower() && t.Id != id))
        {
            return BadRequest("Ya existe una tecnolgía con ese nombre.");
        }

        tecnologiaEnDb.Nombre = tecnologiaDTO.Nombre;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Tecnología actualizada correctamente." });
    }

    //DELETE: api/tecnologias/id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTecnologia(int id)
    {
        var tecnologia = await _context.Tecnologias.FindAsync(id);
        if (tecnologia == null) return NotFound("La tenología no se encontro.");

        _context.Tecnologias.Remove(tecnologia);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = $"La tecnología '{tecnologia.Nombre}' ha sido eliminada correctamente."
        });
    }
}