using System.ComponentModel.DataAnnotations;

namespace apbd_cw9.Models.DTOs;

public class WarehouseDTO
{
    [Required]
    [Range(0, int.MaxValue)]
    public int IdProduct { get; set; }
    [Range(0, int.MaxValue)]
    public int IdWarehouse { get; set; }
    [Range(0, int.MaxValue)]
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}