using AutoMapper;
using Common.Dto;
using Common.enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System.Diagnostics;


namespace Service.Services
{
    public class SongService(ITagRepository tagRepository , IClassificationDataCache cache, IJobRepository jobRepository, ISongRepository songRepository, IWebHostEnvironment environment, IMapper mapper) :ISongService
    {
        private readonly IClassificationDataCache _cache = cache;
        private readonly ISongRepository _songRepository = songRepository;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IJobRepository _jobRepository = jobRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ITagRepository _tagRepository = tagRepository;


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

            // שומרים את הקטגוריה הישנה (יכולה להיות null)
            int? oldCategoryId = song.CategoryID;

            // אוספים את תדירויות התגיות של השיר לדיקשנרי
            var tagFrequencies = song.TagsFrequencies
                .ToDictionary(t => t.TagID, t => t.Frequency);

            // עדכון הקטגוריה בבסיס הנתונים
            await _songRepository.UpdateCategoryAsync(songId, newCategoryId);

            // עדכון ה-Cache בצורה חכמה על פי המצב הקודם
            if (oldCategoryId.HasValue)
            {
                // תרחיש 1: לשיר הייתה קטגוריה, מעבירים אותה מהישנה לחדשה
                _cache.ReassignSong(oldCategoryId.Value, newCategoryId, tagFrequencies);
            }
            else
            {
                // תרחיש 2: לשיר לא הייתה קטגוריה (היה null). 
                // משתמשים במתודה הקיימת שלך שפשוט מוסיפה את נתוני השיר לקטגוריה החדשה ומעדכנת מונים
                _cache.UpdateCacheWithNewSong(newCategoryId, tagFrequencies);
            }
        }

        public async Task<List<SongSearchResultDto>> SearchAsync(string? title, string? artist, int? categoryId, int? tagId)
        {
            var songs = await _songRepository.SearchAsync(title, artist, categoryId, tagId);
            return _mapper.Map<List<SongSearchResultDto>>(songs);
        }

        public async Task<List<TagDto>> GetAllTagsAsync()
        {
            var tags = await _tagRepository.GetAll();
            return _mapper.Map<List<TagDto>>(tags);
        }
        public async Task<Song?> GetSongByIdAsync(int songId)
        {
            return await _songRepository.GetById(songId);
        }

        public async Task<bool> DeleteSongAsync(int songId)
        {
            var song = await _songRepository.GetById(songId);
            if (song == null) return false;
            await _songRepository.DeleteItem(songId);
            return true;
        }

        public async Task<List<Song>> UploadMultipleSongsAsync(List<IFormFile> files, int uploaderId)
        {
            var tasks = files.Select(file =>
            {
                var dto = new SongUploadDto { SongFile = file };
                return UploadAndSaveSongAsync(dto, uploaderId);
            });
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}
