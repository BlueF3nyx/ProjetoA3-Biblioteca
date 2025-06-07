using Xunit;
using System.Threading.Tasks;
using BibliotecaAPP.Core.Models;
using BibliotecaAPP.Core.Data;

public class MembroIntegrationTests : IAsyncLifetime
{
    private readonly MembroRepository _repo = new();
    private int _membroId;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_membroId > 0)
        {
            // Tenta remover o membro criado pelo teste; ignora se já foi apagado.
            await _repo.ExcluirAsync(_membroId);
        }
    }

    [Fact(DisplayName = "Adicionar, Buscar, Atualizar e Excluir Membro com sucesso")]
    public async Task CRUD_Completo_Membro()
    {
        // 1. Adicionar novo membro
        var novoMembro = new Membro
        {
            Nome = "Teste Integracao",
            CPF = "12345678901",
            Telefone = "(99)99999-9999",
            Email = "emailDeTeste@teste.com"
        };
        await _repo.AdicionarAsync(novoMembro);

        // 2. Buscar todos para pegar o ID recém inserido
        var todos = await _repo.ObterTodosAsync();
        var membroInserido = todos.LastOrDefault(m => m.Nome == novoMembro.Nome && m.CPF == novoMembro.CPF);
        Assert.NotNull(membroInserido);
        _membroId = membroInserido.ID;

        // 3. Atualizar dados
        membroInserido.Telefone = "(88)88888-8888";
        membroInserido.Email = "novoemail@teste.com";
        await _repo.AtualizarAsync(membroInserido);

        // 4. Buscar novamente e validar atualização
        var todos2 = await _repo.ObterTodosAsync();
        var membroAtualizado = todos2.FirstOrDefault(m => m.ID == _membroId);
        Assert.NotNull(membroAtualizado);
        Assert.Equal("(88)88888-8888", membroAtualizado!.Telefone);
        Assert.Equal("novoemail@teste.com", membroAtualizado.Email);

        // 5. Excluir membro
        await _repo.ExcluirAsync(_membroId);

        // 6. Validar remoção
        var todos3 = await _repo.ObterTodosAsync();
        Assert.DoesNotContain(todos3, m => m.ID == _membroId);
        _membroId = 0; // Para evitar tentar limpar duas vezes
    }

    [Fact(DisplayName = "Buscar Membro inexistente retorna null")]
    public async Task Buscar_Membro_Inexistente()
    {
        var todos = await _repo.ObterTodosAsync();
        var membro = todos.FirstOrDefault(m => m.ID == -99999);
        Assert.Null(membro);
    }
}
