using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Crowdfunding.Application.Common.Interfaces;

namespace Crowdfunding.Infrastructure.Persistence;

public class DapperContext : IDapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is not defined.");
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
