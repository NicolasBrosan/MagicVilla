using AutoMapper;
using MagicVilla_API.Datos;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public VillaController(ILogger<VillaController> logger, ApplicationDbContext dbContext, IMapper mapper)//---> Se realiza la inyección de dependencia
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
        }




        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
        {
            _logger.LogInformation("Obtener las Villas");

            IEnumerable<Villa> villaList = await _dbContext.Villas.ToListAsync();

            return Ok(_mapper.Map<IEnumerable<VillaDto>>(villaList));

        }

        [HttpGet("id", Name = "GetVilla")]//----> "GetVilla" es el nombre de la ruta
        [ProducesResponseType(StatusCodes.Status200OK)]//----> Documenta el status
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDto>> GetVilla(int id)
        {
            if(id == 0)
            {
                _logger.LogError("Error al traer Villa con Id", + id);
                return BadRequest();//----> Se encuentra mal la solicitud. Cod 400.
            }

            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if(villa == null)
            {
                return NotFound();//----> No encontrado. Cod 404.
            }

            return Ok(_mapper.Map<VillaDto>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDto>> CrearVilla([FromBody] VillaCreateDto createDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if( await _dbContext.Villas.FirstOrDefaultAsync(v => v.Nombre.ToLower() == createDto.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La Villa con ese nombre ya existe!");//---> Validación personalizada. 1er param nombre de la validación y el 2do param el msj q quiero mostrar.
                return BadRequest(ModelState);
            }

            if(createDto == null)
            {
                return BadRequest(createDto);
            }

            Villa modelo = _mapper.Map<Villa>(createDto);

            await _dbContext.Villas.AddAsync(modelo);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetVilla", new {id = modelo.Id}, modelo);//----> Se dirige a la ruta que le indicamos
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla (int id)
        {
            if(id == 0)
            {
                return BadRequest();
            }

            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.Id == id);
            if(villa == null)
            {
                return NotFound();
            }

            _dbContext.Villas.Remove(villa);
            await _dbContext.SaveChangesAsync();

            return NoContent();

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto updateDto)
        {
            if(updateDto == null || id != updateDto.Id)
            {
                return BadRequest();
            }

            Villa modelo = _mapper.Map<Villa>(updateDto);

            _dbContext.Villas.Update(modelo);
            await _dbContext.SaveChangesAsync();
            return NoContent();            
        }


        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        {
            if (patchDto == null || id == 0)
            {
                return BadRequest();
            }

            //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);

            var villa = await _dbContext.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);//---> AsNoTracking() => No rastrea los cambios. Esto significa que cualquier cambio realizado en los objetos no se reflejará en la base de datos al guardar los cambios.

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            if(villa == null)
            {
                return BadRequest();
            }

            patchDto.ApplyTo(villaDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Villa modelo = _mapper.Map<Villa>(villaDto);

            _dbContext.Villas.Update(modelo);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
