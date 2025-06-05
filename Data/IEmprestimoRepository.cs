
using BibliotecaAPP.Models;


namespace BibliotecaAPP.Data
{
    public interface IEmprestimoRepository
    {
        Task<int> AdicionarAsync(Emprestimo emprestimo);
        Task<List<Emprestimo>> ObterTodosAsync();
        Task<List<EmprestimoDetalhado>> ObterRelatorioEmprestimosAsync(DateTime dataInicio, DateTime dataFim, string tipoRelatorio);
        Task<List<EmprestimoDetalhado>> ObterTodosComDetalhesAsync();
        Task<Emprestimo?> ObterPorIdAsync(int id);
        Task<bool> AtualizarAsync(Emprestimo emprestimo);
        Task<bool> ExcluirAsync(int id);
        Task<bool> RealizarDevolucaoAsync(int emprestimoId, string estadoLivro, string? justificativa);
        Task<List<EmprestimoDetalhado>> ObterEmprestimosAtivosPorMembroAsync(int membroId);

        Task<bool> MembroTemEmprestimosAsync(int membroId);
    }
}
