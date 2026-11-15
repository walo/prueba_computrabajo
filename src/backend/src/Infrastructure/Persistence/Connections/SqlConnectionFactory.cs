using System.Data;
using Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Connections;

public sealed class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString = ConnectionStringResolver.Resolve(configuration);

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
