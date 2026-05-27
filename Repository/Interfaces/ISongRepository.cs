using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface ISongRepository: IRepository<Song>
    {
        Task UpdateSongResultAsync(int songId, string lyrics, int? categoryId, Dictionary<Tag, int> finalTags);
        Task<Song> GetSongWithDetails(int id);
        Task<List<Song>> SearchAsync(string? query, int? categoryId);
        Task<List<Song>> GetSongsByCategory(int categoryId);
        Task<Song?> GetByIdWithTagsAsync(int songId);
        Task UpdateCategoryAsync(int songId, int newCategoryId);
    }
}