using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public CategoryRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "SELECT * FROM categories ORDER BY id ASC";
            return await connection.QueryAsync<Category>(sql);
        }
    }
}
