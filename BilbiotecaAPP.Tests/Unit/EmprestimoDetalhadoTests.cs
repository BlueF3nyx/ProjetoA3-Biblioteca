using BibliotecaAPP.Core.Models;
using Xunit;
using System;

namespace BibliotecaAPP.Test.UnitTests
{
    public class EmprestimoDetalhadoTests
    {
        [Fact]
        public void StatusExibicao_Devolvido()
        {
            var model = new EmprestimoDetalhado
            {
                DataDevolucaoReal = DateTime.Today,
                DataDevolucaoPrevista = DateTime.Today
            };
            Assert.Equal("Devolvido", model.StatusExibicao);
        }

        [Fact]
        public void DiasAtraso_Atrasado()
        {
            var hoje = DateTime.Today;
            var model = new EmprestimoDetalhado
            {
                DataDevolucaoPrevista = hoje.AddDays(-3),
                DataDevolucaoReal = null
            };
            Assert.Equal(3, model.DiasAtraso);
        }
    }
}
