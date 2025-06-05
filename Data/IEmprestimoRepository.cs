
using BibliotecaAPP.Models;

namespace BibliotecaAPP.Data
{
    public interface IEmprestimoRepository
    {
        Task AdicionarAsync(Emprestimo emprestimo);
       
        Task<List<EmprestimoDetalhado>> ObterEmprestimosAtivosPorMembroAsync(int membroId);
  
        Task<bool> RealizarDevolucaoAsync(int emprestimoId, string estadoLivro, string? justificativa);
    }
}
