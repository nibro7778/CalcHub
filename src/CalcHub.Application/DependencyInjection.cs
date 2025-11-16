using CalcHub.Application.Services;
using CalcHub.Application.Usecases;
using Microsoft.Extensions.DependencyInjection;

namespace CalcHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ICcsCalculatorService, CcsCalculatorService>();

            return services;
        }
    }
}
