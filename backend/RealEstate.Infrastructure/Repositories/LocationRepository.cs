using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public LocationRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "SELECT * FROM locations ORDER BY parent_id ASC, id ASC";
            return await connection.QueryAsync<Location>(sql);
        }
    }
}
