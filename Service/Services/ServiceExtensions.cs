using Microsoft.Extensions.DependencyInjection;
using Service.Interfaces;
using Repository.Repositories;
using Common.Dto;


namespace Service.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddRepository();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();


            // השארנו את ה-Processor ואת ה-VocalSeparator כשירותים רגילים
            services.AddScoped<ILyricsProcessor, LyricsProcessor>();
            services.AddScoped<IVocalSeparatorService, VocalSeparatorService>();

            // הגדרת ה-HttpClient הייעודי עבור ה-Demucs API בשם שלו
            services.AddHttpClient("DemucsApi", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5); // חשוב עבור פעולות הפרדה ארוכות
            });

            return services;
        }
    }
}
