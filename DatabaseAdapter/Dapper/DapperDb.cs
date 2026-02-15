using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace DatabaseAdapter.Dapper;

public class DapperDb : IDapperDb, IDisposable
{
    private readonly string _connectionString;

    private IDbConnection? Connection { get; set; }

    private IDbConnection Db
    {
        get
        {
            Connection ??= new SqlConnection(_connectionString);
            return Connection;
        }
    }
    
    private IDbTransaction? Transaction { get; set; }

    public DapperDb(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDatabaseConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidConfigurationException("Missing connection string");
        }

        _connectionString = connectionString;
    }

    public void BeginTransaction()
    {
        Db.Open();
        Transaction = Db.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (Transaction is null)
        {
            throw new InvalidConfigurationException("BeginTransaction must be called before Commit");
        }

        Transaction.Commit();
        Transaction.Dispose();
        Db.Close();
    }

    public void RollbackTransaction()
    {
        if (Transaction is null)
        {
            throw new InvalidConfigurationException("BeginTransaction must be called before Rollback");
        }

        Transaction.Rollback();
        Transaction.Dispose();
        Db.Close();
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        return await Db.ExecuteScalarAsync<T>(sql, parameters, transaction: Transaction);
    }

    public async Task ExecuteAsync(string sql, object? parameters = null)
    {
        await Db.ExecuteAsync(sql, parameters, transaction: Transaction);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        return await Db.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction: Transaction);
    }

    public void Dispose()
    {
        Connection?.Dispose();
        Transaction?.Dispose();
    }
}