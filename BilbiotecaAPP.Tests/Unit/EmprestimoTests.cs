using BibliotecaAPP.Core.Models;
using Xunit;
using System;

namespace BibliotecaAPP.Test.UnitTests
{
    public class EmprestimoTests
    {
        [Fact]
        public void OverdueStatusText_Devolvido_RetornaDevolvido()
        {
            var model = new Emprestimo
            {
                DataDevolucaoPrevista = DateTime.Today,
                DataDevolucaoReal = DateTime.Today
            };
            Assert.Equal("Devolvido", model.OverdueStatusText);
        }

        [Fact]
        public void OverdueStatusText_Atrasado()
        {
            var model = new Emprestimo
            {
                DataDevolucaoPrevista = DateTime.Today.AddDays(-5)
            };
            Assert.StartsWith("Atrasado", model.OverdueStatusText);
        }

        // Adicione mais para cada cenário
    }
}
