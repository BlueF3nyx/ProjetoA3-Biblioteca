using BibliotecaAPP.Core.Models;
using Xunit;

namespace BibliotecaAPP.Test.UnitTests
{
    public class FuncionarioTests
    {
        [Fact]
        public void Pode_Criar_Com_Propriedades()
        {
            var funcionario = new Funcionario
            {
                Id = 2,
                Nome = "Sandra",
                Email = "sandra2202@gmail.com",
                Senha = "123456"
            };
            Assert.Equal(2, funcionario.Id);
            Assert.Equal("Sandra", funcionario.Nome);
            Assert.Equal("sandra2202@gmail.com", funcionario.Email);
            Assert.Equal("123456", funcionario.Senha);
        }

        [Fact]
        public void Funcionario_Permite_Valores_Nulos()
        {
            var funcionario = new Funcionario();
            Assert.Null(funcionario.Nome);
            Assert.Null(funcionario.Email);
            Assert.Null(funcionario.Senha);
        }
    }
}
