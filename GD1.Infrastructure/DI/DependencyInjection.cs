using Dapper;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Application.Interfaces.Services;
using GD1.Application.Common.Interfaces;
using GD1.Domain.Interfaces;
using GD1.Infrastructure.Data;
using GD1.Infrastructure.Repositories;
using GD1.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace GD1.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IDbConnection>(sp =>
                new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUserReadRepository, UserReadRepository>();
            services.AddScoped<IManagerReadRepository, ManagerReadRepository>();

            services.AddHttpClient<IVehicleService, VehicleService>();
            // External Vehicle API (Local JSON)
            services.AddScoped<IAuthService, AuthService>();

            // AI and File Services
            services.AddHttpClient();
            services.AddScoped<IOcrService, TesseractOcrService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IVehicleService, VehicleService>();

            services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
            services.AddScoped<IPaymentService, RazorpayService>();
            return services;
        }
    }
}
