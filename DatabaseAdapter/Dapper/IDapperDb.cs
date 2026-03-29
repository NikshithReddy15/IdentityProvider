namespace DatabaseAdapter.Dapper;

public interface IDapperDb
{
    void BeginTransaction();

    void CommitTransaction();

    void RollbackTransaction();
    
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null);

    Task ExecuteAsync(string sql, object? parameters = null);

    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
}