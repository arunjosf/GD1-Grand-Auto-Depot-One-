using Dapper;
using FluentValidation;
using GD1.Api.Middleware;
using GD1.Api.Services;
using GD1.Application.Features.Auth.Commands;
using GD1.Application.Features.FranchiseApplication.Commands;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using GD1.Infrastructure;
using GD1.Infrastructure.Data;
using GD1.Infrastructure.Data;
using GD1.Infrastructure.Repositories;
using GD1.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add this — prevents BrowserRefresh from crashing on file picker open
builder.WebHost.UseSetting("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "");

// CRITICAL: In .NET 8, a background service exception stops the entire host by default.
// This prevents ANY background service crash from taking down the backend.
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});


// Clear default claim mapping to use standard names
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration
        .GetConnectionString("DefaultConnection")));

builder.Services.AddInfrastructure(builder.Configuration);

// Register Dapper Enum Handlers
SqlMapper.AddTypeHandler(new DapperEnumHandler<FranchiseStatus>());
SqlMapper.AddTypeHandler(new DapperEnumHandler<InspectionDecision>());
SqlMapper.AddTypeHandler(new DapperEnumHandler<MaintenanceTaskType>());
SqlMapper.AddTypeHandler(new DapperEnumHandler<MaintenanceTaskStatus>());



builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>));

builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
builder.Services.AddScoped<IFranchiseReadRepository, FranchiseReadRepository>();
builder.Services.AddScoped<IVehicleReadRepository, VehicleReadRepository>();
builder.Services.AddScoped<IBookingReadRepository, BookingReadRepository>();
builder.Services.AddScoped<IPickupReadRepository, PickupReadRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddHttpClient<ISmsService, SmsService>();
builder.Services.AddHttpClient(); // Registers IHttpClientFactory globally
builder.Services.AddHttpClient<GD1.Application.Interfaces.IGeocodingService, GD1.Infrastructure.Services.GeocodingService>();
builder.Services.AddScoped<GD1.Application.Interfaces.Services.IPdfGeneratorService, GD1.Infrastructure.Services.PdfGeneratorService>();
builder.Services.AddHostedService<UnverifiedUserCleanupService>();
builder.Services.AddHostedService<BookingCleanupService>();
//builder.Services.AddHostedService<GD1.Infrastructure.Services.WeeklyMaintenanceService>();

builder.Services.AddScoped<GD1.Application.Interfaces.Services.INotificationService, GD1.Api.Services.NotificationService>();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(GD1.Application.Features.Auth.Commands.LoginCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(GD1.Application.Common.Behaviors.ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(GD1.Application.Features.Auth.Commands.LoginCommand).Assembly);



var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");

builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub",
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VehicleOwner", p => p.RequireRole("VehicleOwner"));
    options.AddPolicy("Agent", p => p.RequireRole("Agent"));
    options.AddPolicy("Admin", p => p.RequireRole("Admin"));
});


builder.Services.AddCors(opt =>
    opt.AddPolicy("Frontend", policy =>
        policy.SetIsOriginAllowed(_ => true) // Allow any origin for easier testing
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// Increase file upload limit (100 MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; 
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.Exception?.Message ?? e.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(GD1.Application.Common.BaseResponse<object>.Fail(string.Join("\n", errors)));
        };
    });

builder.Services.AddSignalR();

// Register Hosted Services
builder.Services.AddHostedService<MonthlyRevenueNotificationService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "GD1 API", Version = "v1" });
    opt.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    opt.SchemaFilter<EditVehicleRequestSchemaFilter>();

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here."
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

});


        var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

app.UseCors("Frontend");
app.UseMiddleware<ExceptionMiddleware>();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GD1.Api.Hubs.TrackingHub>("/hubs/tracking");
app.MapHub<GD1.Api.Hubs.NotificationHub>("/hubs/notification");

    app.Run();

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/Uploads"
});

public class EditVehicleRequestSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type == typeof(GD1.Application.Features.Vehicle.DTOs.EditVehicleRequest))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["brand"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["model"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["year"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["registrationNo"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["color"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["fuelType"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["vehicleType"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["ownerIdProofUrl"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["vehicleRcUrl"] = new Microsoft.OpenApi.Any.OpenApiNull()
            };
        }
    }
}

