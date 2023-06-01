using AutoMapper;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using MagicVilla_API.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumeroVillaController : ControllerBase
    {
        private readonly ILogger<NumeroVillaController> _logger;
        private readonly IVillaRepository _villaRepo;
        private readonly INumeroVillaRepository _numeroRepo;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public NumeroVillaController(ILogger<NumeroVillaController> logger, IVillaRepository villaRepo, INumeroVillaRepository numeroRepo, IMapper mapper)//---> Se realiza la inyección de dependencia
        {
            _logger = logger;
            _villaRepo = villaRepo;
            _numeroRepo = numeroRepo;
            _mapper = mapper;
            _response = new();
        }




        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetNumeroVillas()
        {
            try
            {

                _logger.LogInformation("Obtener Numeros Villas");

                IEnumerable<NumeroVilla> numeroVillaList = await _numeroRepo.ObtenerTodos();

                _response.Resultado = _mapper.Map<IEnumerable<NumeroVillaDto>>(numeroVillaList);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpGet("id", Name = "GetNumeroVilla")]//----> "GetVilla" es el nombre de la ruta
        [ProducesResponseType(StatusCodes.Status200OK)]//----> Documenta el status
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetNumeroVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer Numero Villa con Id", +id);
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsExitoso=false;
                    return BadRequest(_response);//----> Se encuentra mal la solicitud. Cod 400.
                }

                //var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);

                if (numeroVilla == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso=false;
                    return NotFound(_response);//----> No encontrado. Cod 404.
                }
                _response.Resultado = _mapper.Map<NumeroVillaDto>(numeroVilla);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CrearNumeroVilla([FromBody] NumeroVillaCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _numeroRepo.Obtener(v => v.VillaNo == createDto.VillaNo) != null)
                {
                    ModelState.AddModelError("NombreExiste", "El numero de Villa con ese nombre ya existe!");//---> Validación personalizada. 1er param nombre de la validación y el 2do param el msj q quiero mostrar.
                    return BadRequest(ModelState);
                }

                if(await _villaRepo.Obtener(v => v.Id == createDto.VillaId) == null)
                {
                    ModelState.AddModelError("ClaveForanea", "El Id de la Villa no ya existe!");
                    return BadRequest(ModelState);
                }

                if (createDto == null)
                {
                    return BadRequest(createDto);
                }

                NumeroVilla modelo = _mapper.Map<NumeroVilla>(createDto);

                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;

                await _numeroRepo.Crear(modelo);
                _response.Resultado = modelo;
                _response.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetNumeroVilla", new { id = modelo.VillaNo }, _response);//----> Se dirige a la ruta que le indicamos
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNumeroVilla(int id)
        {
            try
            {

                if (id == 0)
                {
                    _response.IsExitoso = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var numeroVilla = await _numeroRepo.Obtener(v => v.VillaNo == id);
                if (numeroVilla == null)
                {
                    _response.IsExitoso = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                await _numeroRepo.Remover(numeroVilla);

                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateNumeroVilla(int id, [FromBody] NumeroVillaUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null || id != updateDto.VillaNo)
                {
                    _response.IsExitoso = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if (await _villaRepo.Obtener(v => v.Id == updateDto.VillaId) == null)
                {
                    ModelState.AddModelError("ClaveForenea", "El Id de la Villa no existe!");
                    return BadRequest(ModelState);
                }

                NumeroVilla modelo = _mapper.Map<NumeroVilla>(updateDto);

                await _numeroRepo.Actualizar(modelo);
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_response);
        }
        
    }
}
