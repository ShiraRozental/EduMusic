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

            services.AddScoped<ILyricsProcessor, LyricsProcessor>();
            services.AddScoped<IVocalSeparatorService, VocalSeparatorService>();
            //services.AddScoped<ILyricsClassifierService, LyricsClassifierService>();

            return services;
        }
    }
}
