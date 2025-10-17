using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MALA PRÁCTICA: Cadena de conexión hardcodeada y expuesta
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=BadDatabase;User Id=sa;Password=123456;TrustServerCertificate=true;"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// MALA PRÁCTICA: Swagger en producción
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// MALA PRÁCTICA: Todo en un solo archivo, sin separación de responsabilidades
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Person> Persons { get; set; }
    
    // MALA PRÁCTICA: Sin configuración de entidades, sin índices, sin restricciones
}

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public PersonController(AppDbContext context)
    {
        _context = context;
    }
    
    // MALA PRÁCTICA: Sin async/await, sin manejo de errores, retorna toda la información
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            // MALA PRÁCTICA: Obtiene TODOS los registros sin paginación
            var persons = _context.Persons.ToList();
            return Ok(persons);
        }
        catch (Exception ex)
        {
            // MALA PRÁCTICA: Expone detalles internos del error
            return BadRequest($"Error interno del servidor: {ex.Message} - {ex.StackTrace}");
        }
    }
    
    // MALA PRÁCTICA: Sin validación de parámetros
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        // MALA PRÁCTICA: Sin validación si id es válido
        var person = _context.Persons.Find(id);
        
        // MALA PRÁCTICA: Retorna null en lugar de 404
        return Ok(person);
    }
    
    // MALA PRÁCTICA: Sin validación de modelo, sin DTOs
    [HttpPost]
    public IActionResult Create(Person person)
    {
        // MALA PRÁCTICA: Sin validaciones
        person.CreatedDate = DateTime.Now;
        person.IsActive = true;
        
        _context.Persons.Add(person);
        _context.SaveChanges(); // MALA PRÁCTICA: Sin async
        
        // MALA PRÁCTICA: Retorna toda la entidad con datos sensibles
        return Ok(person);
    }
    
    // MALA PRÁCTICA: Update completo sin validaciones
    [HttpPut("{id}")]
    public IActionResult Update(int id, Person person)
    {
        // MALA PRÁCTICA: No verifica si existe
        var existingPerson = _context.Persons.Find(id);
        
        // MALA PRÁCTICA: Asignación directa sin validación
        existingPerson.Name = person.Name;
        existingPerson.Email = person.Email;
        existingPerson.Age = person.Age;
        existingPerson.Phone = person.Phone;
        existingPerson.Address = person.Address;
        
        _context.SaveChanges();
        
        return Ok(existingPerson);
    }
    
    // MALA PRÁCTICA: Delete físico sin validaciones
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var person = _context.Persons.Find(id);
        
        // MALA PRÁCTICA: No verifica si existe
        _context.Persons.Remove(person);
        _context.SaveChanges();
        
        return Ok("Eliminado correctamente");
    }
    
    // MALA PRÁCTICA: Endpoint adicional que hace queries innecesarios
    [HttpGet("search/{name}")]
    public IActionResult SearchByName(string name)
    {
        // MALA PRÁCTICA: Query ineficiente, sin índices
        var results = new List<Person>();
        var allPersons = _context.Persons.ToList(); // Trae TODOS los registros
        
        foreach (var p in allPersons)
        {
            if (p.Name.Contains(name)) // MALA PRÁCTICA: No usa LIKE en SQL
            {
                results.Add(p);
            }
        }
        
        return Ok(results);
    }
    
    // MALA PRÁCTICA: Endpoint que expone información sensible del sistema
    [HttpGet("debug/database")]
    public IActionResult GetDatabaseInfo()
    {
        var info = new
        {
            ConnectionString = _context.Database.GetConnectionString(),
            DatabaseName = _context.Database.GetDbConnection().Database,
            ServerVersion = _context.Database.GetDbConnection().ServerVersion,
            AllTables = _context.Model.GetEntityTypes().Select(t => t.GetTableName()).ToList()
        };
        
        return Ok(info);
    }
    
    // MALA PRÁCTICA: Endpoint sin autorización que hace operaciones peligrosas
    [HttpDelete("deleteall")]
    public IActionResult DeleteAll()
    {
        var allPersons = _context.Persons.ToList();
        _context.Persons.RemoveRange(allPersons);
        _context.SaveChanges();
        
        return Ok($"Se eliminaron {allPersons.Count} registros");
    }
}