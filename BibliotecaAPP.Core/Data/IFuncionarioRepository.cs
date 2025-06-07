using BibliotecaAPP.Core.Models;


namespace BibliotecaAPP.Core.Data
{
    public interface IFuncionarioRepository
    {
        Task<Funcionario> AutenticarAsync(string email, string senha);
    }

}
