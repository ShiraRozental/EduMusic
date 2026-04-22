using Microsoft.Extensions.DependencyInjection;
using Service.Interfaces;
using Repository.Repositories;


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

            return services;
        }
    }
}
