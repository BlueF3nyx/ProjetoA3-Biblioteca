using System.Collections.ObjectModel;
using BibliotecaAPP.Core.Models; 

using BibliotecaAPP.Core.Data; 

namespace BibliotecaAPP.Views
{
    public partial class GestaoDevolucoes : ContentPage
    {
        private ObservableCollection<Membro> _membros;
        
        private ObservableCollection<EmprestimoDetalhado> _emprestimosDoMembro;
        
        private EmprestimoDetalhado _emprestimoSelecionado; // Para manter a referência do empréstimo selecionado

        
        private readonly IMembroRepository _membroRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;

        
        public GestaoDevolucoes(IMembroRepository membroRepository, IEmprestimoRepository emprestimoRepository)
        {
            InitializeComponent();

            
            _membroRepository = membroRepository;
            _emprestimoRepository = emprestimoRepository;

            _membros = new ObservableCollection<Membro>();
            
            _emprestimosDoMembro = new ObservableCollection<EmprestimoDetalhado>();

            
            emprestimosListView.ItemsSource = _emprestimosDoMembro;
           
            membroPicker.ItemDisplayBinding = new Binding("Nome");
            membroPicker.ItemsSource = _membros; 

            // Ocultar as seções de detalhes e botões inicialmente
            HideLoanDetails();
            EmptyStateEmprestimosLabel.IsVisible = false; // Oculto inicialmente

            LoadMembrosAsync(); // Carregar membros ao inicializar a página
        }

        // Método para carregar todos os membros para o Picker
        private async void LoadMembrosAsync()
        {
            try
            {
                // TODO: ✅ Substitua a simulação pela sua lógica real de banco de dados usando o Repositório
                var membrosDoBanco = await _membroRepository.ObterTodosAsync();

                _membros.Clear();
                foreach (var membro in membrosDoBanco)
                {
                    _membros.Add(membro);
                }
                // O ItemsSource já foi definido no construtor, apenas atualizamos a coleção
                // membroPicker.ItemsSource = _membros; // Esta linha não é mais necessária aqui
            }
            catch (Exception ex)
            {
                // TODO: Tratar erros de banco de dados
                await DisplayAlert("Erro", $"Ocorreu um erro ao carregar membros: {ex.Message}", "OK");
            }
        }

        // Evento disparado quando um membro é selecionado no Picker
        private async void OnMembroPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            // Limpar seleção anterior e ocultar detalhes
            emprestimosListView.SelectedItem = null;
            ClearLoanDetails();
            HideLoanDetails();

            if (selectedIndex != -1)
            {
                var selectedMembro = (Membro)picker.SelectedItem;
                // TODO: ✅ Chame o método do seu Repositório de Empréstimos para obter os ativos do membro
                await LoadEmprestimosDoMembroAsync(selectedMembro.ID);

                // Atualiza a visibilidade do estado vazio após carregar
                EmptyStateEmprestimosLabel.IsVisible = _emprestimosDoMembro.Count == 0;
            }
            else
            {
                _emprestimosDoMembro.Clear(); // Limpar a lista se nenhum membro for selecionado
                EmptyStateEmprestimosLabel.IsVisible = true; // Mostrar estado vazio
            }
        }

        // Método para carregar os empréstimos ativos de um membro
        // TODO: ✅ Este método agora carrega EmprestimoDetalhado
        private async Task LoadEmprestimosDoMembroAsync(int membroId)
        {
            try
            {
                // TODO: ✅ Substitua a simulação pela sua lógica real de banco de dados usando o Repositório
                // Chame o método que retorna EmprestimoDetalhado
                var emprestimosAtivos = await _emprestimoRepository.ObterEmprestimosAtivosPorMembroAsync(membroId);

                _emprestimosDoMembro.Clear();
                foreach (var emprestimo in emprestimosAtivos)
                {
                    _emprestimosDoMembro.Add(emprestimo);
                }
            }
            catch (Exception ex)
            {
                // TODO: Tratar erros de banco de dados
                await DisplayAlert("Erro", $"Ocorreu um erro ao carregar empréstimos: {ex.Message}", "OK");
            }
        }

        // Evento disparado quando um empréstimo é selecionado na ListView
        private void OnEmprestimoSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // TODO: ✅ Ajuste o casting para EmprestimoDetalhado
            _emprestimoSelecionado = e.SelectedItem as EmprestimoDetalhado;

            if (_emprestimoSelecionado != null)
            {
                // Exibir os detalhes do empréstimo selecionado
                lblTituloLivro.Text = _emprestimoSelecionado.TituloLivro;

                // TODO: ✅ O nome do membro já está disponível no EmprestimoDetalhado
                lblNomeMembro.Text = _emprestimoSelecionado.NomeMembro ?? "Membro não encontrado"; // Exibe o nome ou um placeholder

                lblDataDevolucaoPrevista.Text = _emprestimoSelecionado.DataDevolucaoPrevista.ToString("dd/MM/yyyy");

                // Atualizar o status de atraso e a cor (assumindo que EmprestimoDetalhado tem essas propriedades)
                lblAtraso.Text = _emprestimoSelecionado.StatusExibicao; 
 
                lblAtraso.TextColor = Colors.White; 

                
                LoanDetailsFrame.IsVisible = true;
                BookConditionFrame.IsVisible = true;
                ButtonsGrid.IsVisible = true;

               
                estadoLivroPicker.SelectedIndex = -1;
                justificativaEditor.Text = string.Empty;
            }
            else
            {
                // Nenhum empréstimo selecionado, ocultar detalhes
                ClearLoanDetails();
                HideLoanDetails();
            }
        }

        // Evento disparado quando o botão "Confirmar Devolução" é clicado
        private async void OnConfirmarDevolucaoClicked(object sender, EventArgs e)
        {
            if (_emprestimoSelecionado == null)
            {
                await DisplayAlert("Erro", "Nenhum empréstimo selecionado para devolver.", "OK");
                return;
            }

            // Obter o estado do livro e a justificativa
            string estadoLivro = estadoLivroPicker.SelectedItem?.ToString();
            string justificativa = justificativaEditor.Text;

            if (string.IsNullOrWhiteSpace(estadoLivro))
            {
                await DisplayAlert("Atenção", "Por favor, selecione o estado do livro.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirmar Devolução", $"Deseja realmente marcar o livro '{_emprestimoSelecionado.TituloLivro}' como devolvido?", "Sim", "Não");

            if (confirm)
            {
                try
                {
                    // TODO: ✅ Chame o método do seu Repositório para realizar a devolução
                    // O método RealizarDevolucaoAsync no seu repositório espera emprestimoId, estadoLivro, justificativa
                    bool sucesso = await _emprestimoRepository.RealizarDevolucaoAsync(
                        _emprestimoSelecionado.EmprestimoId, // ✅ Use a propriedade EmprestimoId do EmprestimoDetalhado
                        estadoLivro,
                        justificativa
                    );

                    if (sucesso)
                    {
                        // Recarrega a lista para refletir a mudança (o item devolvido deve sumir da lista de pendentes)
                        if (membroPicker.SelectedItem is Membro selectedMembro)
                        {
                            await LoadEmprestimosDoMembroAsync(selectedMembro.ID);
                            EmptyStateEmprestimosLabel.IsVisible = _emprestimosDoMembro.Count == 0;
                        }

                        // Limpar e ocultar detalhes após a devolução
                        emprestimosListView.SelectedItem = null;
                        ClearLoanDetails();
                        HideLoanDetails();

                        await DisplayAlert("Sucesso", "Livro marcado como devolvido!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Falha ao marcar o livro como devolvido.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Tratar erros de banco de dados
                    await DisplayAlert("Erro", $"Ocorreu um erro ao processar a devolução: {ex.Message}", "OK");
                }
            }
        }

        // Evento disparado quando o botão "Cancelar" é clicado
        private void OnCancelarClicked(object sender, EventArgs e)
        {
            // Limpar seleção e ocultar detalhes
            emprestimosListView.SelectedItem = null;
            ClearLoanDetails();
            HideLoanDetails();
        }

        // Método auxiliar para limpar os campos de detalhes
        private void ClearLoanDetails()
        {
            _emprestimoSelecionado = null;
            lblTituloLivro.Text = "-";
            lblNomeMembro.Text = "-";
            lblDataDevolucaoPrevista.Text = "--/--/----";
            lblAtraso.Text = "Sem informações";
            frameAtraso.BackgroundColor = Colors.Transparent; // Cor transparente
            lblAtraso.TextColor = Colors.Black; // Cor neutra
            estadoLivroPicker.SelectedIndex = -1;
            justificativaEditor.Text = string.Empty;
        }

        // Método auxiliar para ocultar as seções de detalhes e botões
        private void HideLoanDetails()
        {
            LoanDetailsFrame.IsVisible = false;
            BookConditionFrame.IsVisible = false;
            ButtonsGrid.IsVisible = false;
        }
    }
}
