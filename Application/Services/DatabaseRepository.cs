/*
*	<copyright file="DatabaseRepository">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/26/2025 1:32:10 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend_sec_dev.Application.Services
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly AppDbContext db;

        public DatabaseRepository(AppDbContext db)
        {
            this.db = db;
        }

        public void Add<T>(T entity) where T : class
        {
            this.db.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            this.db.Remove(entity);
        }
        public void Update<T>(T entity) where T : class
        {
            this.db.Update(entity);
        }

        public void UpdateRange<T>(List<T> entities) where T : class
        {
            this.db.UpdateRange(entities);
        }

        public async Task<T?> GetById<T>(Guid id) where T : class
        {
            return await this.db.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetAll<T>() where T : class
        {
            return await this.db.Set<T>().ToListAsync();
        }

        public async Task<List<T>> Where<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await this.db.Set<T>().Where(expression).ToListAsync();
        }


        public async Task<int> SaveChanges()
        {
            return await this.db.SaveChangesAsync();
        }

        public async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await this.db.Set<T>().FirstOrDefaultAsync(expression);
        }


    }
}