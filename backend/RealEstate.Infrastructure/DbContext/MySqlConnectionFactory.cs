using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.DbContext
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in application configuration.");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
