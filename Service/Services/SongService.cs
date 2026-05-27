using AutoMapper;
using Common.Dto;
using Common.enums;
using Microsoft.AspNetCore.Hosting;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using System.Diagnostics;


namespace Service.Services
{
    public class SongService(IClassificationDataCache cache, IJobRepository jobRepository, ISongRepository songRepository, IWebHostEnvironment environment, IMapper mapper) :ISongService
    {
        private readonly IClassificationDataCache _cache = cache;
        private readonly ISongRepository _songRepository = songRepository;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IJobRepository _jobRepository = jobRepository;
        private readonly IMapper _mapper = mapper;


        public async Task<Song> UploadAndSaveSongAsync(SongUploadDto dto, int uploaderId)
        {
            if (dto.SongFile == null || dto.SongFile.Length == 0)
                throw new ArgumentException("The audio file is invalid or empty.");

            // save the file to the wwwroot/uploads/audio folder
            string uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "audio");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.SongFile.FileName)}";
            string absolutePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(absolutePath, FileMode.Create))
                await dto.SongFile.CopyToAsync(stream);

            // read metadata using TagLib#
            using var tagFile = TagLib.File.Create(absolutePath);
            string title = dto.Title ?? tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(dto.SongFile.FileName);
            string artist = dto.Artist ?? tagFile.Tag.FirstPerformer ?? "Unknown";
            int duration = (int)tagFile.Properties.Duration.TotalSeconds;

            // creating the song entity
            var song = new Song
            {
                Title = title,
                Artist = artist,
                Duration = duration,
                UploaderID = uploaderId,
                FilePath = $"uploads/audio/{uniqueFileName}",
                Status = SongStatus.Pending,
                UploadDate = DateTime.UtcNow,
            };

            await songRepository.AddItem(song);

            // creating the job for worker processing
            await jobRepository.AddJobAsync(new JobState
            {
                Id = Guid.NewGuid(),
                SongID = song.SongID, 
                Status = JobStatus.Queued,
                CreatedAt = DateTime.UtcNow
            });

            return song;
        }

        public async Task ReassignCategoryAsync(int songId, int newCategoryId, int adminId)
        {
            var song = await _songRepository.GetByIdWithTagsAsync(songId)
                ?? throw new Exception($"Song {songId} not found");

            if (song.CategoryID == newCategoryId) return; 

            int oldCategoryId = song.CategoryID
                ?? throw new Exception("Song has no current category");

            // collect tag frequencies for cache update
            var tagFrequencies = song.TagsFrequencies
                .ToDictionary(t => t.TagID, t => t.Frequency);

            // update the song's category in the database
            await _songRepository.UpdateCategoryAsync(songId, newCategoryId);

            // update the cache to reflect the category change
            _cache.ReassignSong(oldCategoryId, newCategoryId, tagFrequencies);
        }

        public async Task<List<SongSearchResultDto>> SearchAsync(string? query, int? categoryId)
        {
            var songs = await _songRepository.SearchAsync(query, categoryId);
            return _mapper.Map<List<SongSearchResultDto>>(songs);
        }
    }
}
