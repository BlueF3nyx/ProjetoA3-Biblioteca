﻿using BibliotecaAPP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BibliotecaAPP.Data
{
    public interface IMembroRepository
    {
        Task<List<Membro>> ObterTodosAsync();
        Task AdicionarAsync(Membro membro);

        Task AtualizarAsync(Membro membro);  
        Task ExcluirAsync(int id);
        
    }
}
