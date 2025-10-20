using BadApiExample.Models;

namespace BadApiExample.Data.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByEmailAsync(string email);
    Task<IEnumerable<Person>> SearchByNameAsync(string name);
    Task<IEnumerable<Person>> GetActivePersonsAsync();
    Task<(IEnumerable<Person> Items, int TotalCount)> SearchAsync(
        string? name = null,
        string? email = null,
        int? minAge = null,
        int? maxAge = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "Id",
        string sortDirection = "asc");
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}