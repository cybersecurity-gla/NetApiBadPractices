// MALA PRÁCTICA: Controlador adicional con más ejemplos de lo que NO hacer
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class BadExamplesController : ControllerBase
{
    // MALA PRÁCTICA: Campo público con cadena de conexión
    public static string CONNECTION_STRING = "Server=localhost;Database=BadDatabase;User Id=sa;Password=123456;TrustServerCertificate=true;";
    
    // MALA PRÁCTICA: SQL Injection vulnerable
    [HttpGet("search")]
    public IActionResult SearchPersons(string name, string email)
    {
        var sql = $"SELECT * FROM Persons WHERE Name LIKE '%{name}%' AND Email = '{email}'";
        
        var persons = new List<object>();
        using (var connection = new SqlConnection(CONNECTION_STRING))
        {
            connection.Open();
            using (var command = new SqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        persons.Add(new
                        {
                            Id = reader["Id"],
                            Name = reader["Name"],
                            Email = reader["Email"],
                            Age = reader["Age"],
                            Phone = reader["Phone"],
                            Address = reader["Address"]
                        });
                    }
                }
            }
        }
        
        return Ok(persons);
    }
    
    // MALA PRÁCTICA: Endpoint que expone contraseñas y información sensible
    [HttpGet("admin/config")]
    public IActionResult GetSystemConfig()
    {
        var config = new
        {
            DatabasePassword = "123456",
            SecretKey = "super-secret-key-2024",
            AdminEmail = "admin@company.com",
            AdminPassword = "admin123",
            ServerPath = Environment.CurrentDirectory,
            Environment = Environment.GetEnvironmentVariables(),
            ConnectionString = CONNECTION_STRING
        };
        
        return Ok(config);
    }
    
    // MALA PRÁCTICA: Método que no valida entrada y causa memory leaks
    [HttpPost("upload")]
    public IActionResult UploadData([FromBody] List<Person> persons)
    {
        // MALA PRÁCTICA: Sin límite de tamaño, sin validación
        var errors = new StringBuilder();
        
        foreach (var person in persons) // Podría ser millones de registros
        {
            try
            {
                // MALA PRÁCTICA: Crear contexto en loop
                using (var context = new AppDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(CONNECTION_STRING).Options))
                {
                    context.Persons.Add(person);
                    context.SaveChanges(); // MALA PRÁCTICA: SaveChanges en cada iteración
                }
            }
            catch (Exception ex)
            {
                errors.AppendLine($"Error con {person.Name}: {ex.Message}");
            }
        }
        
        // MALA PRÁCTICA: Retorna errores sensibles
        return Ok(new { 
            Message = "Procesado", 
            Errors = errors.ToString(),
            TotalProcessed = persons.Count 
        });
    }
    
    // MALA PRÁCTICA: Endpoint que ejecuta código arbitrario
    [HttpPost("execute")]
    public IActionResult ExecuteCommand(string command)
    {
        try
        {
            // MALA PRÁCTICA: Ejecuta cualquier comando SQL
            using (var connection = new SqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (var cmd = new SqlCommand(command, connection))
                {
                    var result = cmd.ExecuteScalar();
                    return Ok(new { Result = result?.ToString() });
                }
            }
        }
        catch (Exception ex)
        {
            return Ok(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
    
    // MALA PRÁCTICA: Endpoint sin límite de rate que consume recursos
    [HttpGet("stress")]
    public IActionResult StressTest()
    {
        var results = new List<object>();
        
        // MALA PRÁCTICA: Loop infinito potencial
        for (int i = 0; i < 10000; i++)
        {
            using (var context = new AppDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(CONNECTION_STRING).Options))
            {
                var count = context.Persons.Count(); // Query en cada iteración
                results.Add(new { Iteration = i, Count = count });
            }
        }
        
        return Ok(results);
    }
    
    // MALA PRÁCTICA: Endpoint que modifica la base de datos con GET
    [HttpGet("delete/{id}")]
    public IActionResult DeletePersonWithGet(int id)
    {
        using (var context = new AppDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(CONNECTION_STRING).Options))
        {
            var person = context.Persons.Find(id);
            if (person != null)
            {
                context.Persons.Remove(person);
                context.SaveChanges();
            }
        }
        
        return Ok("Persona eliminada");
    }
    
    // MALA PRÁCTICA: Endpoint que retorna datos privados sin autorización
    [HttpGet("private-data")]
    public IActionResult GetPrivateData()
    {
        return Ok(new
        {
            CreditCards = new[] { "4532-1234-5678-9012", "5555-4444-3333-2222" },
            SocialSecurityNumbers = new[] { "123-45-6789", "987-65-4321" },
            Passwords = new[] { "password123", "admin2024" },
            PersonalEmails = new[] { "personal@gmail.com", "secret@yahoo.com" }
        });
    }
}