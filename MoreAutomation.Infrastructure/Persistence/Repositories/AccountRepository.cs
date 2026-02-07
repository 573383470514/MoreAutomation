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
        private readonly string _connectionString;

        public AccountRepository()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NIGHTHAVEN");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string dbPath = Path.Combine(folder, "data.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SortIndex INTEGER,
                    AccountNumber INTEGER NOT NULL UNIQUE,
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
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Accounts ORDER BY SortIndex ASC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var account = new Account(reader.GetInt64(2))
                {
                    Id = reader.GetInt32(0),
                    SortIndex = reader.GetInt32(1),
                    Password = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    GroupId = reader.GetInt32(4),
                    Note = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    IsMaster = reader.GetInt32(6) == 1,
                    ProxyPort = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                };
                list.Add(account);
            }

            return list;
        }

        public async Task AddAsync(Account account)
        {
            ArgumentNullException.ThrowIfNull(account);

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
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

            try
            {
                await command.ExecuteNonQueryAsync();

                // 将插入后的自增 Id 回写到对象上，便于上层使用
                try
                {
                    using var idCmd = connection.CreateCommand();
                    idCmd.CommandText = "SELECT last_insert_rowid();";
                    var scalar = await idCmd.ExecuteScalarAsync();
                    if (scalar != null && long.TryParse(scalar.ToString(), out long last))
                    {
                        account.Id = (int)last;
                    }
                }
                catch
                {
                    // 忽略回写失败，插入本身已完成
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                // SQLite unique constraint maps to error code 19
                if (ex.SqliteErrorCode == 19 && ex.Message?.IndexOf("AccountNumber", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new DuplicateAccountException("账号已存在", ex);
                }

                throw new RepositoryException("数据库写入失败", ex);
            }
        }

        public async Task DeleteAsync(long accountNumber)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Accounts WHERE AccountNumber = $acc";
            command.Parameters.AddWithValue("$acc", accountNumber);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                throw new RepositoryException("删除账号失败", ex);
            }
        }

        public async Task UpdateAsync(Account account)
        {
            ArgumentNullException.ThrowIfNull(account);

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
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

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                if (ex.SqliteErrorCode == 19 && ex.Message?.IndexOf("AccountNumber", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new DuplicateAccountException("账号冲突", ex);
                }

                throw new RepositoryException("数据库更新失败", ex);
            }
        }
    }
}
