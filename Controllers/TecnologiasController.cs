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
}