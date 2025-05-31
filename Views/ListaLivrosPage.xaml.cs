using Microsoft.Maui.Controls;
using BibliotecaAppBase.Models;
using System.Collections.ObjectModel;

namespace BibliotecaAppBase.Views;

public partial class ListaLivrosPage : ContentPage
{
    public ObservableCollection<Livro> Livros { get; set; }

    public ListaLivrosPage()
    {
        InitializeComponent();
        Livros = new ObservableCollection<Livro>
        {
            new Livro { ID = 1, Titulo = "O Senhor dos Anéis", Autor = "J.R.R. Tolkien", Categoria = "Fantasia" },
            new Livro { ID = 2, Titulo = "1984", Autor = "George Orwell", Categoria = "Distopia" },
            new Livro { ID = 3, Titulo = "Dom Casmurro", Autor = "Machado de Assis", Categoria = "Clássico" }
        };

        BindingContext = this;
    }
}