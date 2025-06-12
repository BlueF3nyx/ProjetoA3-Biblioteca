using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using MySql.Data.MySqlClient;
using System.Data;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly string _connectionString = "Server=sql.freedb.tech;Port=3306;Database=freedb_BibliotecaDB;Uid=freedb_usuarioBiblioteca;Pwd=jhnD5fZhu&Bz7a&;";

    public async Task<Funcionario?> AutenticarAsync(string email, string senha)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT * FROM Funcionarios WHERE Email = @Email AND Senha = @Senha";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Senha", senha); 

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Funcionario
            {
                Id = reader.GetInt32("Id"),
                Nome = reader.GetString("Nome"),
                Email = reader.GetString("Email"),
                Senha = reader.GetString("Senha"),
            };
        }

        return null;
    }
}
