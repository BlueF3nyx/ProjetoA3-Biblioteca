using Xunit;
using System;
using System.Threading.Tasks;
using BibliotecaAPP.Core.Data;
using BibliotecaAPP.Core.Models;
namespace BibliotecaAPP.Test.IntegrationTests
{
    public class EmprestimoReIntegrationTests : IAsyncLifetime
    {
        private readonly EmprestimoRepository _repo = new();

        // Utilizar um ID (ou range de datas) bem específico nos exemplos para não atrapalhar dados reais.

        private int emprestimoNovoId;

        public async Task InitializeAsync()
        {
            
        }

        public async Task DisposeAsync()
        {
           
            if (emprestimoNovoId != 0)
                await _repo.RemoverEmprestimoAsync(emprestimoNovoId);
        }

        [Fact]
        public async Task Fluxo_Completo_Emprestimo_Novo_Consulta_Atualiza_Devolve_E_Remove()
        {
            // Arrange (garanta que esses IdLivro e IdMembro existem no banco)
            var emprestimo = new Emprestimo
            {
                IdLivro = 11,         //20  
                IdMembro = 15,        //19
                DataEmprestimo = DateTime.Today,
                DataDevolucaoPrevista = DateTime.Today.AddDays(7),
                Status = "Ativo" //emprestado
            };

            // Act 1: Adiciona
            emprestimoNovoId = await _repo.AdicionarAsync(emprestimo);
            Assert.True(emprestimoNovoId > 0);

            // Act 2: Consulta por ID
            var buscado = await _repo.ObterPorIdAsync(emprestimoNovoId);
            Assert.NotNull(buscado);
            Assert.Equal("Ativo", buscado!.Status);
            Assert.Equal(emprestimo.IdLivro, buscado.IdLivro);

            // Act 3: Atualiza
            buscado.Status = "Pendente";
            var atualizado = await _repo.AtualizarAsync(buscado);
            Assert.True(atualizado);

            var verAtualizacao = await _repo.ObterPorIdAsync(emprestimoNovoId);
            Assert.Equal("Pendente", verAtualizacao!.Status);

            // Act 4: Devolve
            bool devolvido = await _repo.RealizarDevolucaoAsync(emprestimoNovoId, "Bom", null);
            Assert.True(devolvido);

            var afterDevolucao = await _repo.ObterPorIdAsync(emprestimoNovoId);
            Assert.NotNull(afterDevolucao!.DataDevolucaoReal);
            Assert.Equal("Devolvido", afterDevolucao.Status);

            // Act 5: Remove
            var removeu = await _repo.ExcluirAsync(emprestimoNovoId);
            Assert.True(removeu);

            // Confirma que removeu mesmo
            var removido = await _repo.ObterPorIdAsync(emprestimoNovoId);
            Assert.Null(removido);

            // Ajusta cleanup
            emprestimoNovoId = 0;
        }
    }
}
