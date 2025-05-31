namespace BibliotecaAppBase.Models;

public class Livro
{
    public int ID { get; set; }
    public string? Titulo { get; set; }
    public string? Autor { get; set; }
    public string? Categoria { get; set; }
    public string? Disponibilidade { get; set; } 
}