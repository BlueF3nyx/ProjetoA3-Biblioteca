using System;
using System.Collections.ObjectModel;
using System.Windows;


namespace BibliotecaAPP.Views
{
    public partial class RegistroEmprestimoPage : Window
    {
        public class LivroEmprestado
        {
            public string? Livro { get; set; }
            public int? Duracao { get; set; }
            public DateTime? DataEmprestimo { get; set; }
            public DateTime? DataDevolucao { get; set; }
        }

        
    }
}
