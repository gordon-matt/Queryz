using System.Linq.Expressions;

namespace Queryz.Services;

public class GenericDataService<TEntity> : IGenericDataService<TEntity> where TEntity : class
{
    #region Private Members

    private static string cacheKey;
    private static string cacheKeyFiltered;
    private readonly IRepository<TEntity> repository;

    #endregion Private Members

    #region Properties

    protected virtual string CacheKey
    {
        get
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = string.Format("Repository_{0}", typeof(TEntity).Name);
            }
            return cacheKey;
        }
    }

    protected virtual string CacheKeyFiltered
    {
        get
        {
            if (string.IsNullOrEmpty(cacheKeyFiltered))
            {
                cacheKeyFiltered = string.Format("Repository_{0}_{{0}}", typeof(TEntity).Name);
            }
            return cacheKeyFiltered;
        }
    }

    #endregion Properties

    #region Constructor

    public GenericDataService(IRepository<TEntity> repository)
    {
        this.repository = repository;
    }

    #endregion Constructor

    #region IGenericDataService<TEntity> Members

    #region Find

    public virtual IEnumerable<TEntity> Find(SearchOptions<TEntity> options) =>
        repository.Find(options);

    public virtual IEnumerable<TResult> Find<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        repository.Find(options, projection);

    public virtual async Task<IEnumerable<TEntity>> FindAsync(SearchOptions<TEntity> options) =>
        await repository.FindAsync(options);

    public virtual async Task<IEnumerable<TResult>> FindAsync<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        await repository.FindAsync(options, projection);

    public virtual TEntity FindOne(params object[] keyValues) =>
        repository.FindOne(keyValues);

    public TEntity FindOne(SearchOptions<TEntity> options) =>
        repository.FindOne(options);

    public TResult FindOne<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        repository.FindOne(options, projection);

    public virtual async Task<TEntity> FindOneAsync(params object[] keyValues) =>
        await repository.FindOneAsync(keyValues);

    public async Task<TEntity> FindOneAsync(SearchOptions<TEntity> options) =>
        await repository.FindOneAsync(options);

    public async Task<TResult> FindOneAsync<TResult>(SearchOptions<TEntity> options, Expression<Func<TEntity, TResult>> projection) =>
        await repository.FindOneAsync(options, projection);

    #endregion Find

    #region Open/Use Connection

    public virtual IRepositoryConnection<TEntity> OpenConnection() => repository.OpenConnection();

    public virtual IRepositoryConnection<TEntity> UseConnection<TOther>(IRepositoryConnection<TOther> connection)
        where TOther : class => repository.UseConnection(connection);

    #endregion Open/Use Connection

    #region Count

    public virtual int Count() => repository.Count();

    public virtual int Count(Expression<Func<TEntity, bool>> countExpression) => repository.Count(countExpression);

    public virtual async Task<int> CountAsync() => await repository.CountAsync();

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> countExpression) => await repository.CountAsync(countExpression);

    #endregion Count

    #region Delete

    public virtual int DeleteAll()
    {
        int rowsAffected = repository.DeleteAll();
        return rowsAffected;
    }

    public virtual int Delete(TEntity entity)
    {
        int rowsAffected = repository.Delete(entity);
        return rowsAffected;
    }

    public virtual int Delete(IEnumerable<TEntity> entities)
    {
        int rowsAffected = repository.Delete(entities);
        return rowsAffected;
    }

    public virtual int Delete(Expression<Func<TEntity, bool>> filterExpression)
    {
        int rowsAffected = repository.Delete(filterExpression);
        return rowsAffected;
    }

    public virtual int Delete(IQueryable<TEntity> query)
    {
        int rowsAffected = repository.Delete(query);
        return rowsAffected;
    }

    public virtual async Task<int> DeleteAllAsync() => await repository.DeleteAllAsync();

    public virtual async Task<int> DeleteAsync(TEntity entity) => await repository.DeleteAsync(entity);

    public virtual async Task<int> DeleteAsync(IEnumerable<TEntity> entities) => await repository.DeleteAsync(entities);

    public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> filterExpression) => await repository.DeleteAsync(filterExpression);

    public virtual async Task<int> DeleteAsync(IQueryable<TEntity> query) => await repository.DeleteAsync(query);

    #endregion Delete

    #region Insert

    public virtual int Insert(TEntity entity)
    {
        int rowsAffected = repository.Insert(entity);
        return rowsAffected;
    }

    public virtual int Insert(IEnumerable<TEntity> entities)
    {
        int rowsAffected = repository.Insert(entities);
        return rowsAffected;
    }

    public virtual async Task<int> InsertAsync(TEntity entity)
    {
        int rowsAffected = await repository.InsertAsync(entity);
        return rowsAffected;
    }

    public virtual async Task<int> InsertAsync(IEnumerable<TEntity> entities)
    {
        int rowsAffected = await repository.InsertAsync(entities);
        return rowsAffected;
    }

    #endregion Insert

    #region Update

    public virtual int Update(TEntity entity)
    {
        int rowsAffected = repository.Update(entity);
        return rowsAffected;
    }

    public virtual int Update(IEnumerable<TEntity> entities)
    {
        int rowsAffected = repository.Update(entities);
        return rowsAffected;
    }

    public virtual async Task<int> UpdateAsync(TEntity entity)
    {
        int rowsAffected = await repository.UpdateAsync(entity);
        return rowsAffected;
    }

    public virtual async Task<int> UpdateAsync(IEnumerable<TEntity> entities)
    {
        int rowsAffected = await repository.UpdateAsync(entities);
        return rowsAffected;
    }

    #endregion Update

    #endregion IGenericDataService<TEntity> Members
}