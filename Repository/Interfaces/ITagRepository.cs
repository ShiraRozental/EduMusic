using System.Threading.Tasks;
using Repository.Entities;

namespace Repository.Interfaces;

// It inherits everything from IRepository<Tag> and adds specific batch methods
public interface ITagRepository : IRepository<Tag>
{
    Task AddWithoutSave(Tag tag);
    Task SaveAsync();
}