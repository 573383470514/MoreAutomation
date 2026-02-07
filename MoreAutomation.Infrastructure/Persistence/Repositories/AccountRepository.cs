using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MoreAutomation.Contracts.Interfaces;
using MoreAutomation.Domain.Entities;

namespace MoreAutomation.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public AccountRepository()
        {
            // 保持原有路径：文档/NIGHTHAVEN/data.db
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NIGHTHAVEN");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            _dbPath = Path.Combine(folder, "data.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SortIndex INTEGER,
                    AccountNumber INTEGER,
                    Password TEXT,
                    GroupId INTEGER,
                    Note TEXT,
                    IsMaster INTEGER,
                    ProxyPort INTEGER
                );";
            command.ExecuteNonQuery();
        }

        public async Task<List<Account>> GetAllAsync()
        {
            var list = new List<Account>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Accounts ORDER BY SortIndex ASC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // 注意：利用 Domain 层的构造函数，确保加载时也符合规则
                var account = new Account(reader.GetInt64(2))
                {
                    Id = reader.GetInt32(0),
                    SortIndex = reader.GetInt32(1),
                    Password = reader.GetString(3),
                    GroupId = reader.GetInt32(4),
                    Note = reader.GetString(5),
                    IsMaster = reader.GetBoolean(6),
                    ProxyPort = reader.GetInt32(7)
                };
                list.Add(account);
            }
            return list;
        }

        public async Task AddAsync(Account account)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Accounts (SortIndex, AccountNumber, Password, GroupId, Note, IsMaster, ProxyPort)
                VALUES ($index, $acc, $pwd, $grp, $note, $master, $port)";

            command.Parameters.AddWithValue("$index", account.SortIndex);
            command.Parameters.AddWithValue("$acc", account.AccountNumber);
            command.Parameters.AddWithValue("$pwd", account.Password);
            command.Parameters.AddWithValue("$grp", account.GroupId);
            command.Parameters.AddWithValue("$note", account.Note);
            command.Parameters.AddWithValue("$master", account.IsMaster ? 1 : 0);
            command.Parameters.AddWithValue("$port", account.ProxyPort);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(long accountNumber)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Accounts WHERE AccountNumber = $acc";
            command.Parameters.AddWithValue("$acc", accountNumber);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Accounts SET 
                SortIndex = $index, Password = $pwd, GroupId = $grp, 
                Note = $note, IsMaster = $master, ProxyPort = $port
                WHERE AccountNumber = $acc";

            command.Parameters.AddWithValue("$index", account.SortIndex);
            command.Parameters.AddWithValue("$acc", account.AccountNumber);
            command.Parameters.AddWithValue("$pwd", account.Password);
            command.Parameters.AddWithValue("$grp", account.GroupId);
            command.Parameters.AddWithValue("$note", account.Note);
            command.Parameters.AddWithValue("$master", account.IsMaster ? 1 : 0);
            command.Parameters.AddWithValue("$port", account.ProxyPort);

            await command.ExecuteNonQueryAsync();
        }
    }
}