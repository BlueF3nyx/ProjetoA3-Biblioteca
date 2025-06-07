using BibliotecaAPP.Core.Models;
using Xunit;
using System;

namespace BibliotecaAPP.Test.UnitTests
{
    public class HistoricoTests
    {
        [Theory]
        [InlineData("Emprestado", " EMPRESTADO")]
        [InlineData("Devolvido", " DEVOLVIDO")]
        [InlineData("Pendente", " PENDENTE")]
        [InlineData("Atrasado", " ATRASADO")]
        [InlineData("Outro", " INDEFINIDO")]
        public void StatusFormatado_Deve_Retornar_Corretamente(string status, string esperado)
        {
            var historico = new Historico
            {
                Status = status
            };
            Assert.Equal(esperado, historico.StatusFormatado);
        }

        [Fact]
        public void FoiDevolvido_True_Quando_DataDevolucaoReal_Preenchida()
        {
            var historico = new Historico
            {
                DataDevolucaoReal = DateTime.Today
            };
            Assert.True(historico.FoiDevolvido);
        }

        [Fact]
        public void FoiDevolvido_False_Quando_DataDevolucaoReal_Null()
        {
            var historico = new Historico
            {
                DataDevolucaoReal = null
            };
            Assert.False(historico.FoiDevolvido);
        }
    }
}
