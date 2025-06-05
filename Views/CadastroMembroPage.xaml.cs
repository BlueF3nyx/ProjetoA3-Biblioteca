using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net.Mail;

namespace BibliotecaAPP.Views
{
    public partial class CadastroMembroPage : ContentPage
    {
        private readonly IMembroRepository _membroRepository;

        private List<Membro> membros;
        private Membro? membroEmEdicao;

        public CadastroMembroPage()
        {
            InitializeComponent();
            _membroRepository = new MembroRepository(); // Ideal: injeção de dependência
            membros = new List<Membro>();
            membroEmEdicao = null;

            // Carrega membros assincronamente (sem aguardar no construtor)
            _ = CarregarMembrosAsync();
        }

        private async Task CarregarMembrosAsync()
        {
            try
            {
                membros = await _membroRepository.ObterTodosAsync();
                MembrosListView.ItemsSource = membros;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar membros: {ex.Message}", "OK");
            }
        }

        private async void OnSalvarMembroClicked(object sender, EventArgs e)
        {
            string nome = NomeEntry.Text?.Trim();
            string cpf = CpfEntry.Text?.Trim();
            string telefone = TelefoneEntry.Text?.Trim();
            string email = EmailEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) ||
                string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            // Validação simples de email
            try
            {
                var mail = new MailAddress(email);
            }
            catch
            {
                await DisplayAlert("Erro", "Email inválido.", "OK");
                return;
            }

            SalvarButton.IsEnabled = false;
            try
            {
                if (membroEmEdicao == null)
                {
                    var novoMembro = new Membro
                    {
                        Nome = nome,
                        CPF = cpf,
                        Telefone = telefone,
                        Email = email
                    };

                    await _membroRepository.AdicionarAsync(novoMembro);
                    await DisplayAlert("Sucesso", "Membro cadastrado com sucesso!", "OK");
                }
                else
                {
                    membroEmEdicao.Nome = nome;
                    membroEmEdicao.CPF = cpf;
                    membroEmEdicao.Telefone = telefone;
                    membroEmEdicao.Email = email;

                    await _membroRepository.AtualizarAsync(membroEmEdicao);
                    await DisplayAlert("Sucesso", "Membro atualizado com sucesso!", "OK");

                    membroEmEdicao = null;
                    CancelarEdicaoButton.IsVisible = false;
                    SalvarButton.Text = "Salvar Membro";
                }

                LimparFormulario();
                await CarregarMembrosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao salvar membro: {ex.Message}", "OK");
            }
            finally
            {
                SalvarButton.IsEnabled = true;
            }
        }

        private void LimparFormulario()
        {
            NomeEntry.Text = "";
            CpfEntry.Text = "";
            TelefoneEntry.Text = "";
            EmailEntry.Text = "";
        }

        private void OnEditarMembroClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var membroSelecionado = button?.BindingContext as Membro;
            if (membroSelecionado == null) return;

            membroEmEdicao = membroSelecionado;

            NomeEntry.Text = membroEmEdicao.Nome;
            CpfEntry.Text = membroEmEdicao.CPF;
            TelefoneEntry.Text = membroEmEdicao.Telefone;
            EmailEntry.Text = membroEmEdicao.Email;

            SalvarButton.Text = "Atualizar Membro";
            CancelarEdicaoButton.IsVisible = true;
        }

        private async void OnExcluirMembroClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var membroSelecionado = button?.BindingContext as Membro;
            if (membroSelecionado == null) return;

            try
            {
                var emprestimoRepository = new EmprestimoRepository();
                var emprestimosAtivos = await emprestimoRepository.ObterEmprestimosAtivosPorMembroAsync(membroSelecionado.ID);

                if (emprestimosAtivos.Count > 0)
                {
                    var livros = string.Join(", ", emprestimosAtivos.Select(e => e.TituloLivro ?? "Sem título"));
                    await DisplayAlert("Erro",
                        $"Não é possível excluir o membro {membroSelecionado.Nome} pois ele possui {emprestimosAtivos.Count} empréstimo(s) ativo(s):\n\n{livros}\n\nPor favor, realize a devolução dos livros antes de excluir o membro.",
                        "OK");
                    return;
                }

                // Se chegou até aqui, vai tentar excluir
                await DisplayAlert("Info", "Nenhum empréstimo ativo encontrado. Tentando excluir...", "OK");

                // Corrigido: ExcluirAsync não retorna bool, então remova a atribuição
                await _membroRepository.ExcluirAsync(membroSelecionado.ID);

                await DisplayAlert("Sucesso", "Membro excluído com sucesso!", "OK");
                await CarregarMembrosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao excluir membro: {ex.Message}", "OK");
            }
        }


        private void OnCancelarEdicaoClicked(object sender, EventArgs e)
        {
            membroEmEdicao = null;
            LimparFormulario();
            SalvarButton.Text = "Salvar Membro";
            CancelarEdicaoButton.IsVisible = false;
        }

        private void OnMembroSelecionado(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;
        }
        private async void OnDevolverLivrosClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var membroSelecionado = button?.BindingContext as Membro;
            if (membroSelecionado == null) return;

            try
            {
                var connectionString = "Server=sql.freedb.tech;Port=3306;Database=freedb_BibliotecaDB;Uid=freedb_usuarioBiblioteca;Pwd=jhnD5fZhu&Bz7a&;";
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                var resultados = new List<string>();

                // 1. Verificar tabela Emprestimos
                var queryEmprestimos = "SELECT COUNT(*) FROM Emprestimos WHERE IdMembro = @membroId";
                using var cmdEmp = new MySqlCommand(queryEmprestimos, connection);
                cmdEmp.Parameters.AddWithValue("@membroId", membroSelecionado.ID);
                var totalEmprestimos = Convert.ToInt32(await cmdEmp.ExecuteScalarAsync());
                resultados.Add($" Emprestimos: {totalEmprestimos} registros");

                // 2. Verificar se existe tabela Reservas
                try
                {
                    var queryReservas = "SELECT COUNT(*) FROM Reservas WHERE IdMembro = @membroId";
                    using var cmdRes = new MySqlCommand(queryReservas, connection);
                    cmdRes.Parameters.AddWithValue("@membroId", membroSelecionado.ID);
                    var totalReservas = Convert.ToInt32(await cmdRes.ExecuteScalarAsync());
                    resultados.Add($" Reservas: {totalReservas} registros");
                }
                catch
                {
                    resultados.Add($" Reservas: Tabela não existe");
                }

                try
                {
                    var queryHistorico = "SELECT COUNT(*) FROM Historico WHERE IdMembro = @membroId";
                    using var cmdHist = new MySqlCommand(queryHistorico, connection);
                    cmdHist.Parameters.AddWithValue("@membroId", membroSelecionado.ID);
                    var totalHistorico = Convert.ToInt32(await cmdHist.ExecuteScalarAsync());
                    resultados.Add($" Historico: {totalHistorico} registros");
                }
                catch
                {
                    resultados.Add($" Historico: Tabela não existe");
                }

                // 5. Mostrar todas as constraints que referenciam a tabela Membros
                try
                {
                    var queryConstraints = @"
                SELECT 
                    TABLE_NAME,
                    COLUMN_NAME,
                    CONSTRAINT_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE REFERENCED_TABLE_NAME = 'Membros' 
                AND REFERENCED_COLUMN_NAME = 'ID'
                AND TABLE_SCHEMA = 'freedb_BibliotecaDB'";

                    using var cmdConstraints = new MySqlCommand(queryConstraints, connection);
                    using var reader = await cmdConstraints.ExecuteReaderAsync();

                    var constraints = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        var tableName = reader.GetString("TABLE_NAME");
                        var columnName = reader.GetString("COLUMN_NAME");
                        var constraintName = reader.GetString("CONSTRAINT_NAME");
                        constraints.Add($"{tableName}.{columnName} ({constraintName})");
                    }

                    if (constraints.Count > 0)
                    {
                        resultados.Add($"\n FOREIGN KEYS encontradas:");
                        resultados.AddRange(constraints);
                    }
                }
                catch (Exception ex)
                {
                    resultados.Add($"❌ Erro ao buscar constraints: {ex.Message}");
                }

                string relatorio = string.Join("\n", resultados);

                await DisplayAlert("🔍 ANÁLISE COMPLETA",
                    $"Membro: {membroSelecionado.Nome} (ID: {membroSelecionado.ID})\n\n" +
                    $"{relatorio}\n\n" +
                    $" SOLUÇÃO: Se Emprestimos > 0, precisamos deletar os registros da tabela Emprestimos primeiro!",
                    "OK");

                // Se tiver empréstimos, oferecer para deletar
                if (totalEmprestimos > 0)
                {
                    bool confirmar = await DisplayAlert(" DELETAR EMPRÉSTIMOS",
                        $"Encontrados {totalEmprestimos} registros na tabela Emprestimos.\n\n" +
                        $"Deseja DELETAR todos os empréstimos deste membro?\n\n" +
                        $" Esta ação é IRREVERSÍVEL!",
                        " sim",
                        " não");

                    if (confirmar)
                    {
                        try
                        {
                            var deleteQuery = "DELETE FROM Emprestimos WHERE IdMembro = @membroId";
                            using var deleteCmd = new MySqlCommand(deleteQuery, connection);
                            deleteCmd.Parameters.AddWithValue("@membroId", membroSelecionado.ID);

                            int deletados = await deleteCmd.ExecuteNonQueryAsync();

                            await DisplayAlert(" SUCESSO",
                                $"{deletados} registro(s) deletado(s) da tabela Emprestimos!\n\n" +
                                $" Agora você pode excluir o membro!",
                                "OK");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert(" ERRO", $"Falha ao deletar empréstimos: {ex.Message}", "OK");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }



    }
}
