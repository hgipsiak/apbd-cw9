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
            var result = await _warehouseService.AddProduct(dto);
            switch (result)
            {
                case -1: return NotFound("Product not found");
                case -2: return NotFound("Warehouse not found");
                case -3: return BadRequest("Incorrect request");
                case -4: return NotFound("Order not found");
            }

            return Ok(result);
        }
    }
}
