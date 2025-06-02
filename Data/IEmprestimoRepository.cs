using BibliotecaAPP.Models;


namespace BibliotecaAPP.Data
{
    public interface IEmprestimoRepository
    {
                     
        Task AdicionarAsync(Emprestimo emprestimo);             
    }
}
