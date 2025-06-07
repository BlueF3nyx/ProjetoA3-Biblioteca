using BibliotecaAPP.Core.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BibliotecaAPP.Core.Data
{
    // Assumindo que IEmprestimoRepository tem as assinaturas corretas
    public class EmprestimoRepository : IEmprestimoRepository
    {
        private readonly string _connectionString = "Server=sql.freedb.tech;Port=3306;Database=freedb_BibliotecaDB;Uid=freedb_usuarioBiblioteca;Pwd=jhnD5fZhu&Bz7a&;";

        public async Task<int> AdicionarAsync(Emprestimo emprestimo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO Emprestimos (IdLivro, IdMembro, DataEmprestimo, DataDevolucaoPrevista, Status, DataDevolucaoReal)
                  VALUES (@IdLivro, @IdMembro, @DataEmprestimo, @DataDevolucaoPrevista, @Status, @DataDevolucaoReal);
                  SELECT LAST_INSERT_ID();";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdLivro", emprestimo.IdLivro);
            command.Parameters.AddWithValue("@IdMembro", emprestimo.IdMembro);
            command.Parameters.AddWithValue("@DataEmprestimo", emprestimo.DataEmprestimo);
            command.Parameters.AddWithValue("@DataDevolucaoPrevista", emprestimo.DataDevolucaoPrevista);
            command.Parameters.AddWithValue("@Status", emprestimo.Status ?? "Ativo");
            command.Parameters.AddWithValue("@DataDevolucaoReal", DBNull.Value);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public async Task<List<EmprestimoDetalhado>> ObterRelatorioEmprestimosAsync(DateTime dataInicio, DateTime dataFim, string tipoRelatorio)
        {
            var emprestimosDetalhado = new List<EmprestimoDetalhado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Ajusta a dataFim para incluir o dia inteiro
            DateTime dataFimAjustada = dataFim.Date.AddDays(1);

            // Base da query com JOINs
            var query = @"
        SELECT e.ID, e.IdLivro, e.IdMembro, e.DataEmprestimo, e.DataDevolucaoPrevista, e.DataDevolucaoReal, e.Status,
               l.Titulo as TituloLivro, m.Nome as NomeMembro
        FROM Emprestimos e
        LEFT JOIN Livros l ON e.IdLivro = l.Id
        LEFT JOIN Membros m ON e.IdMembro = m.Id
        WHERE 1=1 "; // 1=1 é um truque para facilitar a adição de cláusulas AND

            // Adiciona cláusulas WHERE baseadas no tipo de relatório
            switch (tipoRelatorio)
            {
                case "Empréstimos":
                    // Filtra pela DataEmprestimo dentro do período
                    query += " AND e.DataEmprestimo >= @dataInicio AND e.DataEmprestimo < @dataFimAjustada";
                    break;
                case "Devoluções":
                    // Filtra pela DataDevolucaoReal dentro do período e garante que foi devolvido
                    query += " AND e.DataDevolucaoReal IS NOT NULL AND e.DataDevolucaoReal >= @dataInicio AND e.DataDevolucaoReal < @dataFimAjustada";
                    break;
                case "Atrasos":
                    // Filtra pela DataEmprestimo dentro do período E que o empréstimo está atualmente atrasado
                    query += " AND e.DataEmprestimo >= @dataInicio AND e.DataEmprestimo < @dataFimAjustada AND e.DataDevolucaoReal IS NULL AND e.DataDevolucaoPrevista < CURDATE()";
                    break;
                case "Pendentes":
                    // Filtra pela DataEmprestimo dentro do período E que o empréstimo está atualmente ativo/pendente (não devolvido e não atrasado)
                    query += " AND e.DataEmprestimo >= @dataInicio AND e.DataEmprestimo < @dataFimAjustada AND e.DataDevolucaoReal IS NULL AND e.DataDevolucaoPrevista >= CURDATE()";
                    break;
                default:
                    // Se nenhum tipo válido, talvez retornar vazio ou todos no período?
                    // Por enquanto, vamos retornar vazio para tipo desconhecido.
                    return emprestimosDetalhado;
            }

            // Adiciona ordenação
            query += " ORDER BY e.DataEmprestimo DESC";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@dataInicio", dataInicio.Date);
            command.Parameters.AddWithValue("@dataFimAjustada", dataFimAjustada); // Usamos a data ajustada para o fim do período

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Mapeia os dados do reader para o objeto EmprestimoDetalhado
                var dataEmprestimo = reader.IsDBNull("DataEmprestimo") ? DateTime.MinValue : reader.GetDateTime("DataEmprestimo");
                var dataDevolucaoPrevista = reader.IsDBNull("DataDevolucaoPrevista") ? DateTime.MinValue : reader.GetDateTime("DataDevolucaoPrevista");
                var dataDevolucaoReal = reader.IsDBNull("DataDevolucaoReal") ? (DateTime?)null : reader.GetDateTime("DataDevolucaoReal");

                emprestimosDetalhado.Add(new EmprestimoDetalhado
                {
                    EmprestimoId = reader.GetInt32("ID"),
                    LivroId = reader.GetInt32("IdLivro"),
                    MembroId = reader.GetInt32("IdMembro"),
                    DataEmprestimo = dataEmprestimo,
                    DataDevolucaoPrevista = dataDevolucaoPrevista,
                    DataDevolucaoReal = dataDevolucaoReal,
                    Status = reader.IsDBNull("Status") ? "Desconhecido" : reader.GetString("Status"), // Status do banco
                    TituloLivro = reader.IsDBNull("TituloLivro") ? "Livro não encontrado" : reader.GetString("TituloLivro"),
                    NomeMembro = reader.IsDBNull("NomeMembro") ? "Membro não encontrado" : reader.GetString("NomeMembro")
                    // StatusExibicao e DiasAtraso são calculados na própria classe EmprestimoDetalhado
                });
            }

            return emprestimosDetalhado;
        }
        // Este método retorna a lista de Emprestimos (modelo de banco de dados)
        public async Task<List<Emprestimo>> ObterTodosAsync()
        {
            var emprestimos = new List<Emprestimo>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT ID, IdLivro, IdMembro, DataEmprestimo, DataDevolucaoPrevista, DataDevolucaoReal, Status FROM Emprestimos ORDER BY DataEmprestimo DESC";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // *** CORREÇÃO AQUI: Verificar DBNull para DataEmprestimo e DataDevolucaoPrevista ***
                var dataEmprestimo = reader.IsDBNull("DataEmprestimo") ? DateTime.MinValue : reader.GetDateTime("DataEmprestimo");
                var dataDevolucaoPrevista = reader.IsDBNull("DataDevolucaoPrevista") ? DateTime.MinValue : reader.GetDateTime("DataDevolucaoPrevista");
                var dataDevolucaoReal = reader.IsDBNull("DataDevolucaoReal") ? (DateTime?)null : reader.GetDateTime("DataDevolucaoReal");

                emprestimos.Add(new Emprestimo
                {
                    ID = reader.GetInt32("ID"),
                    IdLivro = reader.GetInt32("IdLivro"),
                    IdMembro = reader.GetInt32("IdMembro"),
                    DataEmprestimo = dataEmprestimo,
                    DataDevolucaoPrevista = dataDevolucaoPrevista,
                    DataDevolucaoReal = dataDevolucaoReal,
                    Status = reader.GetString("Status")
                });
            }
            // Retorna uma lista vazia se nenhum registro for encontrado
            return emprestimos;
        }

        // Este método retorna a lista de EmprestimoDetalhado (modelo de exibição)
        public async Task<List<EmprestimoDetalhado>> ObterTodosComDetalhesAsync()
        {
            var emprestimosDetalhado = new List<EmprestimoDetalhado>(); // Lista do modelo DETALHADO
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT e.ID, e.IdLivro, e.IdMembro, e.DataEmprestimo, e.DataDevolucaoPrevista, e.DataDevolucaoReal, e.Status,
                       l.Titulo as TituloLivro, m.Nome as NomeMembro
                FROM Emprestimos e
                LEFT JOIN Livros l ON e.IdLivro = l.Id
                LEFT JOIN Membros m ON e.IdMembro = m.Id
                ORDER BY e.DataEmprestimo DESC";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // *** CORREÇÃO AQUI: Verificar DBNull para DataEmprestimo e DataDevolucaoPrevista ***
                var dataEmprestimo = reader.IsDBNull("DataEmprestimo") ? DateTime.MinValue : reader.GetDateTime("DataEmprestimo");
                var dataDevolucaoPrevista = reader.IsDBNull("DataDevolucaoPrevista") ? DateTime.MinValue : reader.GetDateTime("DataDevolucaoPrevista");
                var dataDevolucaoReal = reader.IsDBNull("DataDevolucaoReal") ? (DateTime?)null : reader.GetDateTime("DataDevolucaoReal");

                // Cria um objeto EmprestimoDetalhado
                emprestimosDetalhado.Add(new EmprestimoDetalhado
                {
                    EmprestimoId = reader.GetInt32("ID"), // Mapeia ID do Emprestimo para EmprestimoId
                    LivroId = reader.GetInt32("IdLivro"),
                    MembroId = reader.GetInt32("IdMembro"),
                    DataEmprestimo = dataEmprestimo,
                    DataDevolucaoPrevista = dataDevolucaoPrevista,
                    DataDevolucaoReal = dataDevolucaoReal,
                    // O status de exibição será calculado na propriedade StatusExibicao do modelo EmprestimoDetalhado
                    // Se precisar do status original do banco, adicione uma propriedade para ele no EmprestimoDetalhado
                    // Ex: StatusBanco = reader.GetString("Status"),
                    // Detalhes extras do JOIN
                    TituloLivro = reader.IsDBNull("TituloLivro") ? "Livro não encontrado" : reader.GetString("TituloLivro"),
                    NomeMembro = reader.IsDBNull("NomeMembro") ? "Membro não encontrado" : reader.GetString("NomeMembro")
                });
            }
            // Retorna uma lista vazia se nenhum registro for encontrado
            return emprestimosDetalhado;
        }

        public async Task<Emprestimo?> ObterPorIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT ID, IdLivro, IdMembro, DataEmprestimo, DataDevolucaoPrevista, DataDevolucaoReal, Status FROM Emprestimos WHERE ID = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                // *** CORREÇÃO AQUI: Verificar DBNull para DataEmprestimo e DataDevolucaoPrevista ***
                var dataEmprestimo = reader.IsDBNull("DataEmprestimo") ? DateTime.MinValue : reader.GetDateTime("DataEmprestimo");
                var dataDevolucaoPrevista = reader.IsDBNull("DataDevolucaoPrevista") ? DateTime.MinValue : reader.GetDateTime("DataDevolucaoPrevista");
                var dataDevolucaoReal = reader.IsDBNull("DataDevolucaoReal") ? (DateTime?)null : reader.GetDateTime("DataDevolucaoReal");

                return new Emprestimo
                {
                    ID = reader.GetInt32("ID"),
                    IdLivro = reader.GetInt32("IdLivro"),
                    IdMembro = reader.GetInt32("IdMembro"),
                    DataEmprestimo = dataEmprestimo,
                    DataDevolucaoPrevista = dataDevolucaoPrevista,
                    DataDevolucaoReal = dataDevolucaoReal,
                    Status = reader.GetString("Status")
                };
            }
            return null;
        }

        // Este método atualiza um objeto Emprestimo (modelo de banco de dados)
        public async Task<bool> AtualizarAsync(Emprestimo emprestimo)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            // Inclua DataDevolucaoPrevista e DataDevolucaoReal no UPDATE
            var query = @"UPDATE Emprestimos
                          SET IdLivro = @IdLivro, IdMembro = @IdMembro,
                              DataEmprestimo = @DataEmprestimo, DataDevolucaoPrevista = @DataDevolucaoPrevista,
                              DataDevolucaoReal = @DataDevolucaoReal, Status = @Status
                          WHERE ID = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", emprestimo.ID);
            command.Parameters.AddWithValue("@IdLivro", emprestimo.IdLivro);
            command.Parameters.AddWithValue("@IdMembro", emprestimo.IdMembro);
            command.Parameters.AddWithValue("@DataEmprestimo", emprestimo.DataEmprestimo);
            command.Parameters.AddWithValue("@DataDevolucaoPrevista", emprestimo.DataDevolucaoPrevista);
            // Trata o valor nulo para DataDevolucaoReal
            command.Parameters.AddWithValue("@DataDevolucaoReal", emprestimo.DataDevolucaoReal.HasValue ? emprestimo.DataDevolucaoReal.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Status", emprestimo.Status ?? "Ativo");

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "DELETE FROM Emprestimos WHERE ID = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }

        // Método específico para realizar a devolução
        public async Task<bool> RealizarDevolucaoAsync(int emprestimoId, string estadoLivro, string? justificativa)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            // Inclua colunas para estado e justificativa se existirem na tabela Emprestimos
            // (Se essas colunas não existem, remova-as da query UPDATE)
            var query = @"UPDATE Emprestimos
                          SET DataDevolucaoReal = @DataDevolucaoReal, Status = 'Devolvido'
                          -- , EstadoLivroDevolucao = @EstadoLivroDevolucao -- Remova se não existir
                          -- , JustificativaDevolucao = @JustificativaDevolucao -- Remova se não existir
                          WHERE ID = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", emprestimoId);
            command.Parameters.AddWithValue("@DataDevolucaoReal", DateTime.Now);
            // Adicione parâmetros para EstadoLivroDevolucao e JustificativaDevolucao SE EXISTIREM na tabela
            // command.Parameters.AddWithValue("@EstadoLivroDevolucao", estadoLivro); // Adicione se existir
            // command.Parameters.AddWithValue("@JustificativaDevolucao", (object)justificativa ?? DBNull.Value); // Adicione se existir

            return await command.ExecuteNonQueryAsync() > 0;
        }

        // Método para obter empréstimos ativos para um membro específico, retornando o modelo detalhado para a UI
        // Método para obter empréstimos ativos para um membro específico, retornando o modelo detalhado para a UI
        public async Task<List<EmprestimoDetalhado>> ObterEmprestimosAtivosPorMembroAsync(int membroId)
        {
            var emprestimos = new List<EmprestimoDetalhado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
        SELECT e.ID, e.IdLivro, e.IdMembro, e.DataEmprestimo, e.DataDevolucaoPrevista, e.DataDevolucaoReal, e.Status,
               l.Titulo as TituloLivro, m.Nome as NomeMembro
        FROM Emprestimos e
        LEFT JOIN Livros l ON e.IdLivro = l.Id
        LEFT JOIN Membros m ON e.IdMembro = m.Id
        WHERE e.IdMembro = @MembroId
        -- ✅ Usar comparação segura para DataDevolucaoReal
        AND (e.DataDevolucaoReal IS NULL OR e.DataDevolucaoReal < '1900-01-01')
        ORDER BY e.DataEmprestimo DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MembroId", membroId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // A leitura com IsDBNull já está correta aqui
                var dataEmprestimo = reader.IsDBNull("DataEmprestimo") ? DateTime.MinValue : reader.GetDateTime("DataEmprestimo");
                var dataDevolucaoPrevista = reader.IsDBNull("DataDevolucaoPrevista") ? DateTime.MinValue : reader.GetDateTime("DataDevolucaoPrevista");
                var dataDevolucaoReal = reader.IsDBNull("DataDevolucaoReal") ? (DateTime?)null : reader.GetDateTime("DataDevolucaoReal");

                emprestimos.Add(new EmprestimoDetalhado
                {
                    EmprestimoId = reader.GetInt32("ID"),
                    LivroId = reader.GetInt32("IdLivro"),
                    MembroId = reader.GetInt32("IdMembro"),
                    TituloLivro = reader.IsDBNull("TituloLivro") ? "Livro não encontrado" : reader.GetString("TituloLivro"),
                    NomeMembro = reader.IsDBNull("NomeMembro") ? "Membro não encontrado" : reader.GetString("NomeMembro"),
                    DataEmprestimo = dataEmprestimo,
                    DataDevolucaoPrevista = dataDevolucaoPrevista,
                    DataDevolucaoReal = dataDevolucaoReal,
                    // ✅ Mapeia o Status do banco para a propriedade Status do EmprestimoDetalhado
                    Status = reader.IsDBNull("Status") ? "Desconhecido" : reader.GetString("Status")
                    // As propriedades DiasAtraso e StatusExibicao serão calculadas automaticamente na classe EmprestimoDetalhado
                });
            }

            // ✅ REMOVA TODO O LOOP FOREACH AQUI!
            // O cálculo de DiasAtraso e StatusExibicao é feito na própria classe EmprestimoDetalhado.

            // Retorna a lista de empréstimos
            return emprestimos;
        }
        public async Task<bool> MembroTemEmprestimosAsync(int membroId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            // Conta quantos empréstimos existem para este membro (ativos ou devolvidos)
            var query = "SELECT COUNT(*) FROM Emprestimos WHERE IdMembro = @MembroId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MembroId", membroId);
            var count = await command.ExecuteScalarAsync();

            // ExecuteScalarAsync retorna um object, convertemos para long e verificamos se é maior que zero
            return Convert.ToInt64(count) > 0;
        }
        public async Task RemoverEmprestimoAsync(int emprestimoId)
        {
            var query = "DELETE FROM Emprestimos WHERE ID = @id";
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", emprestimoId);
            await command.ExecuteNonQueryAsync();
        }
    }
}
