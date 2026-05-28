using Common.Dto;
using Microsoft.AspNetCore.Http;
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
        Task<List<SongSearchResultDto>> SearchAsync(string? title, string? artist, int? categoryId, int? tagId);
        Task<Song?> GetSongByIdAsync(int songId);
        Task<List<TagDto>> GetAllTagsAsync();
        Task<bool> DeleteSongAsync(int songId);
        Task<List<Song>> UploadMultipleSongsAsync(List<IFormFile> files, int uploaderId);

    }
}
