# ProjetoA3-Biblioteca 📚
Sistema de gerenciamento de biblioteca desenvolvido com .NET MAUI, como parte de um projeto acadêmico. O objetivo é permitir que funcionários da biblioteca possam cadastrar livros, membros, empréstimos e acompanhar devoluções e históricos de forma prática e eficiente.
Projeto acadêmico — Universidade Potiguar (UNP)
Curso: Análise e Desenvolvimento de Sistema
Professor: Professor Erivelton De Lima

Protótipo do Figma:
https://www.figma.com/design/aX0b8aFyKexZbkrDkTeOjg/ProjetoSistemaBiblioteca?node-id=0-1&t=psPlqimmqnQ1qxkp-1

## 🛠 Tecnologias Utilizadas

- [.NET MAUI](https://learn.microsoft.com/pt-br/dotnet/maui/) — Framework para criação de apps multiplataforma (Windows, Android, iOS, macOS)
- C# — Linguagem principal da aplicação
- MySQL — Banco de dados relacional
- Entity Framework Core — ORM para manipulação do banco de dados
- XAML — Linguagem de marcação para as interfaces do usuário

## 🎯 Funcionalidades

- **Login de Funcionários**
- **Cadastro de Livros** (com título, autor, ISBN etc.)
- **Cadastro de Membros** (com nome, e-mail, telefone)
- **Registro de Empréstimos** e **Devoluções**
- **Histórico de Empréstimos**
- **Relatórios de uso**
- **Controle de Acesso de Administradores**



## 🚀 Como Rodar o Projeto

1. Clone este repositório:

   ```bash
   git clone https://github.com/seu-usuario/BibliotecaAPP.git

2. Abra o projeto no Visual Studio 2022+ com o .NET MAUI instalado.

3. Configure a string de conexão com seu banco de dados MySQL no arquivo de configuração.

4. Execute as migrations (se estiver usando EF Core):

   ```bash
   dotnet ef database update


5. Rode o projeto
