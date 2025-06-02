using BibliotecaAPP.Models;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace BibliotecaAPP.Data
{
    public class EmprestimoRepository : IEmprestimoRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=BibliotecaDB;Uid=root;Pwd=SuaSenhaAqui;";

        public async Task AdicionarAsync(Emprestimo emprestimo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Emprestimos (IdLivro, IdMembro, DataEmprestimo, DataDevolucao, Status) 
                          VALUES (@IdLivro, @IdMembro, @DataEmprestimo, @DataDevolucao, @Status)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdLivro", emprestimo.IdLivro);
            command.Parameters.AddWithValue("@IdMembro", emprestimo.IdMembro);
            command.Parameters.AddWithValue("@DataEmprestimo", emprestimo.DataEmprestimo);
            command.Parameters.AddWithValue("@DataDevolucao", emprestimo.DataDevolucao);
            command.Parameters.AddWithValue("@Status", emprestimo.Status);

            await command.ExecuteNonQueryAsync();
        }
    }
}
