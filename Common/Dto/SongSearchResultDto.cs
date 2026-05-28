

using Common.enums;

namespace Common.Dto
{
    public class SongSearchResultDto
    {
        public int SongID { get; set; }
        public string Title { get; set; }
        public string? Artist { get; set; }
        public int Duration { get; set; }
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public SongStatus Status { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
