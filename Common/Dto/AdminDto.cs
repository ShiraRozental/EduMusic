using Microsoft.AspNetCore.Http;


namespace Common.Dto
{
    public class AdminDto
    {
        public int AdminID { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
    }
}
