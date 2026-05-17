using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAll(Expression<Func<T, bool>> filter = null);
        Task<T?> GetById(int id);
        Task<T> AddItem(T item);
        Task<T> UpdateItem(int id, T item);
        Task DeleteItem(int id);

    }
}
