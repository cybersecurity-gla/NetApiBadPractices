// MALA PRÁCTICA: Archivo adicional con código duplicado y mal organizado
using Microsoft.EntityFrameworkCore;

public class BadDataService
{
    // MALA PRÁCTICA: Singleton con estado mutable
    public static BadDataService Instance = new();
    
    private string connectionString = "Server=localhost;Database=BadDatabase;User Id=sa;Password=123456;TrustServerCertificate=true;";
    
    // MALA PRÁCTICA: Método que abre múltiples conexiones
    public List<Person> GetAllPersonsSlowly()
    {
        var persons = new List<Person>();
        
        using (var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString).Options))
        {
            var allIds = context.Persons.Select(p => p.Id).ToList();
            
            // MALA PRÁCTICA: N+1 problema
            foreach (var id in allIds)
            {
                using (var newContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connectionString).Options))
                {
                    var person = newContext.Persons.Find(id);
                    if (person != null)
                        persons.Add(person);
                }
            }
        }
        
        return persons;
    }
    
    // MALA PRÁCTICA: Método que no libera recursos
    public void CreatePersonUnsafe(Person person)
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString).Options);
        
        context.Persons.Add(person);
        context.SaveChanges();
        // MALA PRÁCTICA: No dispose del contexto
    }
}

// MALA PRÁCTICA: Extensiones innecesarias y mal implementadas
public static class BadExtensions
{
    public static string ToUnsafeString(this Person person)
    {
        // MALA PRÁCTICA: Exposición de datos sensibles
        return $"Person: {person.Name}, Email: {person.Email}, Phone: {person.Phone}, Address: {person.Address}";
    }
    
    public static Person FromUnsafeString(this string personData)
    {
        // MALA PRÁCTICA: Parsing sin validación
        var parts = personData.Split(',');
        return new Person
        {
            Name = parts[0],
            Email = parts[1],
            Phone = parts[2],
            Address = parts[3]
        };
    }
}