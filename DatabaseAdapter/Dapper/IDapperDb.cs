namespace DatabaseAdapter.Dapper;

public interface IDapperDb
{
    Task<int> ExecuteAsync<T>(string sql, object? parameters = null);

    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);
}