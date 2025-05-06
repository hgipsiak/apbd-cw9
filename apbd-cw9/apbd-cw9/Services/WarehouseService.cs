using System.Data.Common;
using apbd_cw9.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw9.Services;

public class WarehouseService : IWarehouseService
{
    IConfiguration _configuration;

    public async Task<int> AddProduct(WarehouseDTO dto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);

        var res = (int)await command.ExecuteScalarAsync();
        if (res == 0) return -1;

        command.Parameters.Clear();
        command.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);

        res = (int)await command.ExecuteScalarAsync();
        if (res == 0) return -2;

        command.Parameters.Clear();
        command.CommandText =
            "SELECT COUNT(*), Amount, CreatedAt FROM [Order] WHERE IdProduct = @IdProduct GROUP BY Amount, CreatedAt";
        command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
        var reader = await command.ExecuteReaderAsync();

        int amount = reader.GetInt32(reader.GetOrdinal("Amount"));
        DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
        res = (int)await command.ExecuteScalarAsync();
        if (res == 0 || amount != dto.Amount || createdAt < dto.CreatedAt) return -3;

        command.Parameters.Clear();
        command.CommandText = @"SELECT COUNT(*), IdOrder FROM Product_Warehouse 
                WHERE IdProduct = @IdProduct AND IdWarehouse = @IdWarehouse GROUP BY IdOrder";
        command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);

        reader = await command.ExecuteReaderAsync();
        int order = reader.GetInt32(reader.GetOrdinal("IdOrder"));
        res = (int)await command.ExecuteScalarAsync();
        if (res == 0) return -4;
        command.Parameters.Clear();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("IdOrder", order);

            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
            reader = await command.ExecuteReaderAsync();
            int price = reader.GetInt32(reader.GetOrdinal("Price"));
            command.Parameters.Clear();

            command.CommandText =
                @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                  SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", order);
            command.Parameters.AddWithValue("@Amount", dto.Amount);
            command.Parameters.AddWithValue("@Price", dto.Amount * price);
            command.Parameters.AddWithValue("@CreatedAt", dto.CreatedAt);

            await command.ExecuteNonQueryAsync();
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
}