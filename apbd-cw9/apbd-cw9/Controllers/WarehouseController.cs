using apbd_cw9.Exceptions;
using apbd_cw9.Models.DTOs;
using apbd_cw9.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService service)
        {
            _warehouseService = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(WarehouseDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _warehouseService.AddProduct(dto);
                return Created(result.ToString(), result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
        }
    }
}
