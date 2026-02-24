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
        Task<Song> GetSongWithDetails(int id);
        Task<List<Song>> SearchSongs(string query);

        Task<List<Song>> GetSongsByCategory(int categoryId);


    }
}