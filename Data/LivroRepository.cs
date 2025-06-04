using BibliotecaAppBase.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace BibliotecaAPP.Data
{
    public class LivroRepository : ILivroRepository
    {
        private readonly string _connectionString = "Server=sql.freedb.tech;Port=3306;Database=freedb_BibliotecaDB;Uid=freedb_usuarioBiblioteca;Pwd=jhnD5fZhu&Bz7a&;";

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
                    Disponibilidade = reader.GetString("Disponibilidade")  
                });
            }

            return livros;
        }

        public async Task AdicionarAsync(Livro livro)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Livros (Titulo, Autor, Categoria, Disponibilidade)
                          VALUES (@Titulo, @Autor, @Categoria, @Disponibilidade)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Titulo", livro.Titulo);
            command.Parameters.AddWithValue("@Autor", livro.Autor);
            command.Parameters.AddWithValue("@Categoria", livro.Categoria);

            // Enviar o valor de Disponibilidade diretamente, exemplo: "Disponível" ou "Emprestado"
            command.Parameters.AddWithValue("@Disponibilidade", livro.Disponibilidade ?? "Disponível");

            await command.ExecuteNonQueryAsync();
        }

        public async Task AtualizarAsync(Livro livro)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE Livros 
                          SET Titulo = @Titulo, Autor = @Autor, Categoria = @Categoria, Disponibilidade = @Disponibilidade 
                          WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", livro.ID);
            command.Parameters.AddWithValue("@Titulo", livro.Titulo);
            command.Parameters.AddWithValue("@Autor", livro.Autor);
            command.Parameters.AddWithValue("@Categoria", livro.Categoria);
            command.Parameters.AddWithValue("@Disponibilidade", livro.Disponibilidade ?? "Disponível");

            await command.ExecuteNonQueryAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM Livros WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
