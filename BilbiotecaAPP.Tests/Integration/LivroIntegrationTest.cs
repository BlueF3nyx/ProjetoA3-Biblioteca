using Xunit;
using System.Threading.Tasks;
using BibliotecaAPP.Core.Models;
using BibliotecaAPP.Core.Data;

public class LivroIntegrationTests : IAsyncLifetime
{
    private readonly LivroRepository _repo = new();
    private int _livroId;

    // Roda antes de todos os testes
    public Task InitializeAsync() => Task.CompletedTask;

    // Roda depois de todos os testes — limpeza para não poluir
    public async Task DisposeAsync()
    {
        if (_livroId > 0)
        {
            await _repo.ExcluirAsync(_livroId);
        }
    }

    [Fact(DisplayName = "Adicionar, Buscar, Atualizar e Excluir Livro com sucesso")]
    public async Task CRUD_Completo_Livro()
    {
        // 1. Adicionar livro existente
        var novoLivro = new Livro
        {
            Titulo = "Um livro de Teste",
            Autor = "Autor Teste",
            Categoria = "teste",
            Disponibilidade = "Disponível"
        };

        await _repo.AdicionarAsync(novoLivro);

        // Busca todos para pegar o ID do livro recém inserido
        var todos = await _repo.ObterTodosAsync();
        var livroInserido = todos.LastOrDefault(l => l.Titulo == novoLivro.Titulo && l.Autor == novoLivro.Autor);
        Assert.NotNull(livroInserido);
        _livroId = livroInserido!.ID;

        // 2. Buscar por Id
        var buscado = await _repo.ObterPorIdAsync(_livroId);
        Assert.NotNull(buscado);
        Assert.Equal("Um livro de Teste", buscado!.Titulo);

        // 3. Atualizar (título e disponibilidade)
        buscado.Titulo = "Livro Atualizado";
        buscado.Disponibilidade = "Disponível";
        await _repo.AtualizarAsync(buscado);

        var atualizado = await _repo.ObterPorIdAsync(_livroId);
        Assert.Equal("Livro Atualizado", atualizado!.Titulo);
        Assert.Equal("Disponível", atualizado.Disponibilidade);

        // 4. Atualização de disponibilidade específica
        await _repo.AtualizarDisponibilidadeAsync(_livroId, "Disponível");
        var disponivel = await _repo.ObterPorIdAsync(_livroId);
        Assert.Equal("Disponível", disponivel!.Disponibilidade);

        // 5. Excluir (deve funcionar pois não tem empréstimos ativos)
        var (sucesso, mensagem) = await _repo.ExcluirAsync(_livroId);
        Assert.True(sucesso, mensagem);

        // Verifica que não existe mais
        var excluido = await _repo.ObterPorIdAsync(_livroId);
        Assert.Null(excluido);

        // Marca como limpo para evitar duplo-cleanup
        _livroId = 0;
    }


    [Fact(DisplayName = "AtualizarDisponibilidade inválida deve lançar exceção")]
    public async Task Atualizar_Disponibilidade_Invalida()
    {
        // Adiciona novo livro temporário
        var livro = new Livro
        {
            Titulo = "Livro Teste Disponibilidade Inválida",
            Autor = "Autor",
            Categoria = "Categoria",
            Disponibilidade = "Disponível"
        };
        await _repo.AdicionarAsync(livro);
        var todos = await _repo.ObterTodosAsync();
        var livroTemp = todos.LastOrDefault(l => l.Titulo == livro.Titulo);
        Assert.NotNull(livroTemp);
        var tempId = livroTemp.ID;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repo.AtualizarDisponibilidadeAsync(tempId, "Indisponivel")
        );

        // Cleanup
        await _repo.ExcluirAsync(tempId);
    }

    [Fact(DisplayName = "Obter estatísticas retorna informações corretas")]
    public async Task Obter_Estatisticas()
    {
        var (total, disponiveis, emprestados) = await _repo.ObterEstatisticasAsync();
        Assert.True(total > 0); // Espera-se que existam livros cadastrados
        Assert.True(total == disponiveis + emprestados);
    }
}
