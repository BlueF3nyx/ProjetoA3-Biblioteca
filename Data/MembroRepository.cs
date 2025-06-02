using BibliotecaAPP.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace BibliotecaAPP.Data
{
    public class MembroRepository : IMembroRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=BibliotecaDB;Uid=root;Pwd=Fe2ja0@!;";

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
                    Nome = reader.GetString("Nome"),
                    CPF = reader.GetString("CPF"),
                    Telefone = reader.GetString("Telefone"),
                    Email = reader.GetString("Email")
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
    }
}
