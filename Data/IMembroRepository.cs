using BibliotecaAPP.Models;

namespace BibliotecaAPP.Data
{
    public interface IMembroRepository
    {
        Task<List<Membro>> ObterTodosAsync();
        Task AdicionarAsync(Membro membro);
        
    }
}
