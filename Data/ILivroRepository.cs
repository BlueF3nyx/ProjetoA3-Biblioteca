using System.Collections.Generic;
using System.Threading.Tasks;
using BibliotecaAppBase.Models;

namespace BibliotecaAPP.Data
{
    public interface ILivroRepository
    {
        Task<List<Livro>> ObterTodosAsync();
        Task AdicionarAsync(Livro livro);
        Task AtualizarAsync(Livro livro);  
        Task ExcluirAsync(int id);          
    }
}
