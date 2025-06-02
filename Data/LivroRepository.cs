using BibliotecaAppBase.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace BibliotecaAPP.Data
{
    public class LivroRepository : ILivroRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=BibliotecaDB;Uid=root;Pwd=Fe2ja0@!;";

        public async Task<List<Livro>> ObterTodosAsync()
        {
            var livros = new List<Livro>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM Livros";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                livros.Add(new Livro
                {
                    ID = reader.GetInt32("Id"),
                    Titulo = reader.GetString("Titulo"),
                    Autor = reader.GetString("Autor"),
                    Categoria = reader.GetString("Categoria"),
                    Disponibilidade = reader.GetBoolean("Disponivel") ? "Sim" : "Não"
                });
            }

            return livros;
        }

        public async Task AdicionarAsync(Livro livro)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Livros (Titulo, Autor, Categoria, Disponivel)
                          VALUES (@Titulo, @Autor, @Categoria, @Disponivel)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Titulo", livro.Titulo);
            command.Parameters.AddWithValue("@Autor", livro.Autor);
            command.Parameters.AddWithValue("@Categoria", livro.Categoria);
            command.Parameters.AddWithValue("@Disponivel", livro.Disponibilidade == "Sim");

            await command.ExecuteNonQueryAsync();
        }
    }
}
