using BibliotecaAPP.Models;


namespace BibliotecaAPP.Data
{
    public interface IFuncionarioRepository
    {
        Task<Funcionario> AutenticarAsync(string email, string senha);
    }

}
