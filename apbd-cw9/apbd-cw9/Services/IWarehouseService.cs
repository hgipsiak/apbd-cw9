using apbd_cw9.Models.DTOs;

namespace apbd_cw9.Services;

public interface IWarehouseService
{
    Task<int> AddProduct(WarehouseDTO dto);
    Task AddProductProcedure(WarehouseDTO dto);
}