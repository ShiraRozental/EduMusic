using Common.Dto;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ISongService
    {
        Task<Song> UploadAndSaveSongAsync(SongUploadDto dto, int uploaderId);
        Task ReassignCategoryAsync(int songId, int newCategoryId, int adminId);
        Task<List<SongSearchResultDto>> SearchAsync(string? query, int? categoryId);
    }
}
