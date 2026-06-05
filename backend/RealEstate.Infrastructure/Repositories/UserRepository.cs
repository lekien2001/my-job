using System.Data;
using System.Threading.Tasks;
using Dapper;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "SELECT * FROM users WHERE id = @Id LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "SELECT * FROM users WHERE email = @Email LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "SELECT * FROM users WHERE phone_number = @PhoneNumber LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { PhoneNumber = phoneNumber });
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO users (email, phone_number, password_hash, full_name, avatar_url, role, status)
                VALUES (@Email, @PhoneNumber, @PasswordHash, @FullName, @AvatarUrl, @Role, @Status);
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE users 
                SET email = @Email, 
                    phone_number = @PhoneNumber, 
                    password_hash = @PasswordHash, 
                    full_name = @FullName, 
                    avatar_url = @AvatarUrl, 
                    role = @Role, 
                    status = @Status 
                WHERE id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, user);
            return rowsAffected > 0;
        }
    }
}
