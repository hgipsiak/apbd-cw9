using System.Data;
using System.Data.Common;
using apbd_cw9.Exceptions;
using apbd_cw9.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw9.Services;

public class WarehouseService : IWarehouseService
{
    IConfiguration _configuration;

    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddProduct(WarehouseDTO dto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);

            var res = (int)await command.ExecuteScalarAsync();
            if (res == 0) throw new NotFoundException("Product not found");

            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);

            res = (int)await command.ExecuteScalarAsync();
            if (res == 0) throw new NotFoundException("Warehouse not found");

            command.Parameters.Clear();
            command.CommandText =
                "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
            command.Parameters.AddWithValue("@Amount", dto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", dto.CreatedAt);

            res = 0;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                res = reader.GetInt32(0);
            }
            reader.Close();
            if (res == 0) throw new NotFoundException("Order not found");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder", res);
            var ordersInProductWarehouse = (int)await command.ExecuteScalarAsync();
            if (ordersInProductWarehouse != 0) throw new ConflictException("Order already fulfilled");
            command.Parameters.Clear();
            
            command.CommandText = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("IdOrder", res);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            
            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
            await using var reader2 = await command.ExecuteReaderAsync();
            decimal price = 0;
            while (await reader2.ReadAsync())
            {
                price = reader2.GetDecimal(reader2.GetOrdinal("Price"));
            }
            reader2.Close();
            command.Parameters.Clear();

            command.CommandText =
                @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                  SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", res);
            command.Parameters.AddWithValue("@Amount", dto.Amount);
            command.Parameters.AddWithValue("@Price", dto.Amount * price);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            
            var result = await command.ExecuteScalarAsync();

            await transaction.CommitAsync();

            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AddProductProcedure(WarehouseDTO dto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();

        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", dto.Amount);
        command.Parameters.AddWithValue("@CreatedAt", dto.CreatedAt);
        
        await command.ExecuteNonQueryAsync();
    }
}