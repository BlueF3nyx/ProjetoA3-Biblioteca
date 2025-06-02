using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAppBase.Models;
namespace BibliotecaAPP.Data
{
    public interface ILivroRepository
    {
        Task<List<Livro>> ObterTodosAsync();
        Task AdicionarAsync(Livro livro);
    }
}
