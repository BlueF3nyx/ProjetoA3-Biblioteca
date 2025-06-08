using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using BibliotecaAPP.Models;

namespace BibliotecaAPP.Data
{
    public class MembroRepository : IMembroRepository
    {
        private readonly string _connectionString = "Server=sql.freedb.tech;Port=3306;Database=freedb_BibliotecaDB;Uid=freedb_usuarioBiblioteca;Pwd=jhnD5fZhu&Bz7a&;";

        public async Task<List<Membro>> ObterTodosAsync()
        {
            var membros = new List<Membro>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT * FROM Membros";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                membros.Add(new Membro
                {
                    ID = reader.GetInt32("Id"),
                    //verificação IsDBNull para colunas string
                    Nome = reader.IsDBNull("Nome") ? "" : reader.GetString("Nome"),
                    CPF = reader.IsDBNull("CPF") ? "" : reader.GetString("CPF"),
                    Telefone = reader.IsDBNull("Telefone") ? "" : reader.GetString("Telefone"),
                    Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email")
                });
            }
            return membros;
        }

        public async Task AdicionarAsync(Membro membro)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO Membros (Nome, CPF, Telefone, Email)
                          VALUES (@Nome, @CPF, @Telefone, @Email)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nome", membro.Nome);
            command.Parameters.AddWithValue("@CPF", membro.CPF);
            command.Parameters.AddWithValue("@Telefone", membro.Telefone);
            command.Parameters.AddWithValue("@Email", membro.Email);
            await command.ExecuteNonQueryAsync();
        }

        // Método Update
        public async Task AtualizarAsync(Membro membro)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"UPDATE Membros
                          SET Nome = @Nome, CPF = @CPF, Telefone = @Telefone, Email = @Email
                          WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nome", membro.Nome);
            command.Parameters.AddWithValue("@CPF", membro.CPF);
            command.Parameters.AddWithValue("@Telefone", membro.Telefone);
            command.Parameters.AddWithValue("@Email", membro.Email);
            command.Parameters.AddWithValue("@Id", membro.ID);
            await command.ExecuteNonQueryAsync();
        }

        // Método Delete
        public async Task ExcluirAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "DELETE FROM Membros WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
}