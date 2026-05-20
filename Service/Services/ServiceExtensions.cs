using Microsoft.Extensions.DependencyInjection;
using Service.Interfaces;
using Repository.Repositories;
using Common.Dto;
using Repository.Interfaces;


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
            services.AddScoped<IClassificationService, ClassificationService>();
            services.AddScoped<ILyricsProcessor, LyricsProcessor>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ISongService, SongService>();

            services.AddSingleton<IClassificationDataCache, ClassificationDataCache>();

            // for Dmucs vocal separation and Groq API interactions
            services.AddScoped<IVocalSeparatorService, VocalSeparatorService>();
            services.AddHttpClient("DemucsApi", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5); 
            });


            return services;
        }
    }
}
