using Microsoft.EntityFrameworkCore;

namespace RepositoryOverEfCore;

// "We abstract EF Core behind a generic repository and a unit of work,
//  so we follow best practice and can swap the ORM later." – the original author

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    void Commit();
}

public class EfRepository<T>(ShopDbContext db) : IRepository<T> where T : class
{
    public IEnumerable<T> GetAll() => db.Set<T>().ToList();

    public T? GetById(int id) => db.Set<T>().Find(id);

    public void Add(T entity)
    {
        db.Set<T>().Add(entity);
        db.SaveChanges();   // "so the entity gets its Id right away"
    }

    public void Update(T entity)
    {
        db.Set<T>().Update(entity);
        db.SaveChanges();
    }

    public void Delete(T entity)
    {
        db.Set<T>().Remove(entity);
        db.SaveChanges();
    }
}

public class EfUnitOfWork(ShopDbContext db) : IUnitOfWork
{
    public void Commit() => db.SaveChanges();
}
