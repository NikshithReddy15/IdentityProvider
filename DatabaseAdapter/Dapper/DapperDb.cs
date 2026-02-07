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

    public DapperDb(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidConfigurationException("Missing connection string");
        }

        _connectionString = connectionString;
    }

    public async Task<int> ExecuteAsync<T>(string sql, object? parameters = null)
    {
        return await Db.ExecuteAsync(sql, parameters);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        return await Db.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}