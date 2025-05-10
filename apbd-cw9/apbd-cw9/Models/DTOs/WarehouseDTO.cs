using System.ComponentModel.DataAnnotations;

namespace apbd_cw9.Models.DTOs;

public class WarehouseDTO
{
    [Required]
    [Range(0, int.MaxValue)]
    public int IdProduct { get; set; }
    [Required]
    [Range(0, int.MaxValue)]
    public int IdWarehouse { get; set; }
    [Required]
    [Range(0, int.MaxValue)]
    public int Amount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}