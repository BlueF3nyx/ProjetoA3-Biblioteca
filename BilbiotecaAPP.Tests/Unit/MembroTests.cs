using BibliotecaAPP.Core.Models;
using Xunit;

namespace BibliotecaAPP.Test.UnitTests
{
    public class MembroTests
    {
        [Fact]
        public void Deve_Criar_Membro_Com_Propriedades_Preenchidas()
        {
            var membro = new Membro
            {
                ID = 5,
                Nome = "Rafael",
                CPF = "122.230.098-45",
                Telefone = "84993126184",
                Email = "rafaelHRE@gmail.com"
            };

            Assert.Equal(5, membro.ID);
            Assert.Equal("Rafael", membro.Nome);
            Assert.Equal("122.230.098-45", membro.CPF);
            Assert.Equal("84993126184", membro.Telefone);
            Assert.Equal("rafaelHRE@gmail.com", membro.Email);
        }

        [Fact]
        public void Membro_Permite_Valores_Nulos()
        {
            var membro = new Membro();

            Assert.Null(membro.Nome);
            Assert.Null(membro.CPF);
            Assert.Null(membro.Telefone);
            Assert.Null(membro.Email);
        }
    }
}
