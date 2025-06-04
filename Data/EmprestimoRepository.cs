
using BibliotecaAPP.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace BibliotecaAPP.Data
{
    public class EmprestimoRepository : IEmprestimoRepository
    {
        private readonly string _connectionString = "Server=sql.freedb.tech;Port=3306;Database=freedb_BibliotecaDB;Uid=freedb_usuarioBiblioteca;Pwd=jhnD5fZhu&Bz7a&;";


        public async Task AdicionarAsync(Emprestimo emprestimo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // 1. Inserir empréstimo
                var query = @"INSERT INTO Emprestimos (IdLivro, IdMembro, DataEmprestimo, DataDevolucao, Status)
                      VALUES (@IdLivro, @IdMembro, @DataEmprestimo, @DataDevolucao, @Status)";
                using var command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@IdLivro", emprestimo.IdLivro);
                command.Parameters.AddWithValue("@IdMembro", emprestimo.IdMembro);
                command.Parameters.AddWithValue("@DataEmprestimo", emprestimo.DataEmprestimo);
                command.Parameters.AddWithValue("@DataDevolucao", emprestimo.DataDevolucao);
                command.Parameters.AddWithValue("@Status", emprestimo.Status);

                await command.ExecuteNonQueryAsync();

                // 2. Atualizar disponibilidade do livro para "Emprestado"
                var updateLivroCmd = new MySqlCommand(@"
                            UPDATE Livros 
                            SET Disponibilidade = 'Emprestado' 
                            WHERE Id = @IdLivro", connection, transaction);
                updateLivroCmd.Parameters.AddWithValue("@IdLivro", emprestimo.IdLivro);
                await updateLivroCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<EmprestimoDetalhado>> ObterEmprestimosAtivosPorMembroAsync(int membroId)
        {
            var emprestimos = new List<EmprestimoDetalhado>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    e.ID as EmprestimoId,
                    e.IdMembro as MembroId,
                    m.Nome as NomeMembro,
                    e.IdLivro as LivroId,
                    l.Titulo as TituloLivro,
                    e.DataEmprestimo,
                    e.DataDevolucao,
                    e.Status
                FROM Emprestimos e
                INNER JOIN Membros m ON e.IdMembro = m.Id
                INNER JOIN Livros l ON e.IdLivro = l.Id
                WHERE e.IdMembro = @membroId AND e.Status = 'Ativo'
                ORDER BY e.DataDevolucao";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@membroId", membroId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                emprestimos.Add(new EmprestimoDetalhado
                {
                    EmprestimoId = reader.GetInt32("EmprestimoId"),
                    MembroId = reader.GetInt32("MembroId"),
                    NomeMembro = reader.GetString("NomeMembro"),
                    LivroId = reader.GetInt32("LivroId"),
                    TituloLivro = reader.GetString("TituloLivro"),
                    DataEmprestimo = reader.GetDateTime("DataEmprestimo"),
                    DataDevolucao = reader.GetDateTime("DataDevolucao"),
                    Status = reader.GetString("Status")
                });
            }

            return emprestimos;
        }

        public async Task<bool> RealizarDevolucaoAsync(int emprestimoId, string estadoLivro, string? justificativa)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // 1. Buscar ID do livro
                var buscarLivroCmd = new MySqlCommand(
                    "SELECT IdLivro FROM Emprestimos WHERE ID = @emprestimoId",
                    connection, transaction);
                buscarLivroCmd.Parameters.AddWithValue("@emprestimoId", emprestimoId);
                var livroId = await buscarLivroCmd.ExecuteScalarAsync();

                // 2. Atualizar status do empréstimo para "Devolvido"
                var updateEmprestimoCmd = new MySqlCommand(@"
                    UPDATE Emprestimos 
                    SET Status = 'Devolvido' 
                    WHERE ID = @emprestimoId", connection, transaction);
                updateEmprestimoCmd.Parameters.AddWithValue("@emprestimoId", emprestimoId);
                await updateEmprestimoCmd.ExecuteNonQueryAsync();

                // 3. Atualizar disponibilidade do livro (apenas se não perdido)
                if (estadoLivro != "Perdido")
                {
                    var updateLivroCmd = new MySqlCommand(@"
                        UPDATE Livros 
                        SET Disponibilidade = 'Disponível' 
                        WHERE Id = @livroId", connection, transaction);
                    updateLivroCmd.Parameters.AddWithValue("@livroId", livroId);
                    await updateLivroCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
