using BibliotecaAppBase.Models;

namespace BibliotecaAPP.Data
{
    public interface ILivroRepository
    {
        Task<List<Livro>> ObterTodosAsync();
        Task<Livro> ObterPorIdAsync(int id);
        Task AdicionarAsync(Livro livro);
        Task AtualizarAsync(Livro livro);
        Task<(bool sucesso, string mensagem)> ExcluirAsync(int id);
        Task<bool> PodeExcluirAsync(int id);
        Task<(int total, int disponiveis, int emprestados)> ObterEstatisticasAsync();
    }
}
