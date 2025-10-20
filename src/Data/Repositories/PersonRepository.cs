using Microsoft.EntityFrameworkCore;
using BadApiExample.Models;
using System.Linq.Expressions;

namespace BadApiExample.Data.Repositories;

public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<IEnumerable<Person>> SearchByNameAsync(string name)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Person>> GetActivePersonsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Email == email);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<(IEnumerable<Person> Items, int TotalCount)> SearchAsync(
        string? name = null,
        string? email = null,
        int? minAge = null,
        int? maxAge = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "Id",
        string sortDirection = "asc")
    {
        var query = _dbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(p => p.Email.Contains(email));
        }

        if (minAge.HasValue)
        {
            query = query.Where(p => p.Age >= minAge.Value);
        }

        if (maxAge.HasValue)
        {
            query = query.Where(p => p.Age <= maxAge.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDirection);

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    private static IQueryable<Person> ApplySorting(IQueryable<Person> query, string sortBy, string sortDirection)
    {
        var isDescending = sortDirection.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "email" => isDescending ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
            "age" => isDescending ? query.OrderByDescending(p => p.Age) : query.OrderBy(p => p.Age),
            "createddate" => isDescending ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate),
            "isactive" => isDescending ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
            _ => isDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };
    }
}