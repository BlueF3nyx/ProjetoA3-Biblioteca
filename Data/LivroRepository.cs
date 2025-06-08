using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using BibliotecaAPP.Models;

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
                    // ✅ Adicionada verificação IsDBNull para colunas string
                    Titulo = reader.IsDBNull("Titulo") ? "" : reader.GetString("Titulo"),
                    Autor = reader.IsDBNull("Autor") ? "" : reader.GetString("Autor"),
                    Categoria = reader.IsDBNull("Categoria") ? "" : reader.GetString("Categoria"),
                    Disponibilidade = reader.IsDBNull("Disponibilidade") ? "Indefinido" : reader.GetString("Disponibilidade") // Valor padrão diferente para Disponibilidade
                });
            }
            return livros;
        }

        public async Task<Livro?> ObterPorIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT * FROM Livros WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Livro
                {
                    ID = reader.GetInt32("Id"),
                    // ✅ Adicionada verificação IsDBNull para colunas string
                    Titulo = reader.IsDBNull("Titulo") ? "" : reader.GetString("Titulo"),
                    Autor = reader.IsDBNull("Autor") ? "" : reader.GetString("Autor"),
                    Categoria = reader.IsDBNull("Categoria") ? "" : reader.GetString("Categoria"),
                    Disponibilidade = reader.IsDBNull("Disponibilidade") ? "Indefinido" : reader.GetString("Disponibilidade")
                };
            }
            return null;
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

        // ✅ MÉTODO ATUALIZADO COM VERIFICAÇÃO DE SEGURANÇA
        public async Task<bool> PodeExcluirAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            // Verificar se há empréstimos ativos para este livro
            var query = @"SELECT COUNT(*) FROM Emprestimos
                          WHERE IdLivro = @IdLivro AND (Status = 'Ativo' OR Status = 'Atrasado')";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdLivro", id);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count == 0;
        }

        public async Task<(bool sucesso, string mensagem)> ExcluirAsync(int id)
        {
            try
            {
                // 1. Verificar se pode excluir
                if (!await PodeExcluirAsync(id))
                {
                    return (false, "❌ Não é possível excluir este livro.\nExistem empréstimos ativos ou em atraso associados a ele.");
                }
                // 2. Obter dados do livro antes de excluir (para mensagem)
                var livro = await ObterPorIdAsync(id);
                if (livro == null)
                {
                    return (false, "❌ Livro não encontrado.");
                }
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                // 3. Excluir histórico de empréstimos antigos (devolvidos) se existir
                var queryHistorico = "DELETE FROM Emprestimos WHERE IdLivro = @IdLivro AND Status = 'Devolvido'";
                using var commandHistorico = new MySqlCommand(queryHistorico, connection);
                commandHistorico.Parameters.AddWithValue("@IdLivro", id);
                await commandHistorico.ExecuteNonQueryAsync();
                // 4. Excluir o livro
                var queryLivro = "DELETE FROM Livros WHERE Id = @Id";
                using var commandLivro = new MySqlCommand(queryLivro, connection);
                commandLivro.Parameters.AddWithValue("@Id", id);
                int rowsAffected = await commandLivro.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return (true, $"✅ Livro '{livro.Titulo}' excluído com sucesso!");
                }
                else
                {
                    return (false, "❌ Erro ao excluir o livro.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"❌ Erro ao excluir livro: {ex.Message}");
            }
        }

        // Método auxiliar para obter estatísticas
        public async Task<(int total, int disponiveis, int emprestados)> ObterEstatisticasAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"SELECT
                            COUNT(*) as Total,
                            SUM(CASE WHEN Disponibilidade = 'Disponível' THEN 1 ELSE 0 END) as Disponiveis,
                            SUM(CASE WHEN Disponibilidade = 'Emprestado' THEN 1 ELSE 0 END) as Emprestados
                          FROM Livros";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (
                    reader.GetInt32("Total"),
                    reader.GetInt32("Disponiveis"),
                    reader.GetInt32("Emprestados")
                );
            }
            return (0, 0, 0);
        }

        public async Task AtualizarDisponibilidadeAsync(int livroId, string novaDisponibilidade)
        {
            // ✅ VALIDAR se o valor é válido para o ENUM
            var valoresValidos = new[] { "Disponível", "Emprestado", "Atrasado" };
            if (!valoresValidos.Contains(novaDisponibilidade))
            {
                throw new ArgumentException($"Disponibilidade deve ser: {string.Join(", ", valoresValidos)}");
            }
            var query = "UPDATE Livros SET Disponibilidade = @disponibilidade WHERE ID = @id";
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@disponibilidade", novaDisponibilidade);
            command.Parameters.AddWithValue("@id", livroId);
            await command.ExecuteNonQueryAsync();
        }
    }
}