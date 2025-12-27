/*
*	<copyright file="IDatabaseRepository">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/26/2025 1:29:19 PM</date>
*	<description></description>
**/

using System.Linq.Expressions;

namespace Backend_sec_dev.Application.Interfaces
{
    public interface IDatabaseRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void UpdateRange<T>(List<T> entity) where T : class;
        Task<T?> GetById<T>(Guid id) where T : class;
        Task<List<T>> GetAll<T>() where T : class;
        Task<int> SaveChanges();
        Task<List<T>> Where<T>(Expression<Func<T, bool>> expression) where T : class;
        Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression) where T : class;
    }
}