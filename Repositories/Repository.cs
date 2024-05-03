using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TelegramBot.Data;
using TelegramBot.Data;

namespace TelegramBot.Repositories;

public abstract class Repository<T>(ApplicationDbContext context) where T : class
{
    private DbSet<T> _dbSet = context.Set<T>();
    
    public T? Find(int id)
    {
        return _dbSet.Find(id);
    }
    
    public List<T> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null,
                bool isTracking = true)
    {
        IQueryable<T> query = _dbSet;
            
        if (filter != null)
            query = query.Where(filter);
        if (orderBy != null)
            query = orderBy(query);
        if (!isTracking)
            query = query.AsNoTracking();
        else
            query = query.AsTracking();
        if (includeProperties != null)
            foreach (var property in includeProperties.Split(',')) 
                query = query.Include(property);
        return query.ToList();
    }
    
    public T? FirstOrDefault(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool isTracking = true)
    {
        IQueryable<T> query = _dbSet;
            
        if (filter != null)
            query = query.Where(filter);
        query = !isTracking ? query.AsNoTracking() : query.AsTracking();
        if (includeProperties != null)
            foreach (var property in includeProperties.Split(','))
                query = query.Include(property);
    
        return query.FirstOrDefault();
    }
    
    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }
    
    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
    
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
    
    public void Save()
    {
        context.SaveChanges();
    }
}