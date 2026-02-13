namespace DatabaseAdapter.Dapper;

public interface IDapperDb
{
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null);

    Task<int> ExecuteAsync(string sql, object? parameters = null);

    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);
}