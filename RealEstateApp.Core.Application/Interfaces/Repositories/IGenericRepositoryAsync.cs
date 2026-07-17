using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Interfaces.Repositories;

public interface IGenericRepositoryAsync<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<T?> GetByIdWithIncludeAsync(int id, params Expression<Func<T, object>>[] includes);
    Task<IReadOnlyList<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);
}