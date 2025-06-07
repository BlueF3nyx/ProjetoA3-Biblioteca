using BibliotecaAPP.Core.Models;
using Xunit;

namespace BibliotecaAPP.Test.UnitTests
{
    public class LivroTests
    {
        [Fact]
        public void Deve_Criar_Livro_Com_Propriedades_Preenchidas()
        {
            var livro = new Livro
            {
                ID = 1,
                Titulo = "Dom Casmurro",
                Autor = "Machado de Assis",
                Categoria = "Romance",
                Disponibilidade = "Disponível"
            };

            Assert.Equal(1, livro.ID);
            Assert.Equal("Dom Casmurro", livro.Titulo);
            Assert.Equal("Machado de Assis", livro.Autor);
            Assert.Equal("Romance", livro.Categoria);
            Assert.Equal("Disponível", livro.Disponibilidade);
        }

        [Fact]
        public void Livro_Permite_Propriedades_Nulas()
        {
            var livro = new Livro();

            Assert.Null(livro.Titulo);
            Assert.Null(livro.Autor);
            Assert.Null(livro.Categoria);
            Assert.Null(livro.Disponibilidade);
        }
    }
}
