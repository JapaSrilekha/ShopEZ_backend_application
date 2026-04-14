using System.Text;
using ECommerce.API.Data;
using ECommerce.API.Middleware;
using ECommerce.API.Repositories;
using ECommerce.API.Repositories.Interfaces;
using ECommerce.API.Services;
using ECommerce.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using ECommerce.API.DTOs;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //  Database 
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));


        //  Repositories (DI) 
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        //  Services (DI) 
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddHttpContextAccessor();

        //  Controllers 
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Disable automatic 400 — controllers handle validation manually
                options.SuppressModelStateInvalidFilter = true;
            });

        //  CORS 
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ShopEZCors", policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        //  JWT Authentication 
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(
            jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey missing."));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ClockSkew = TimeSpan.Zero
            };

            // Return meaningful JSON for 401 — instead of empty body
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    // Suppress the default 401 response so our middleware body takes effect
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var message = string.IsNullOrEmpty(context.ErrorDescription)
                        ? "You are not authenticated. Please login and provide a valid Bearer token."
                        : $"Authentication failed: {context.ErrorDescription}";

                    var response = ApiResponse<object>.Fail(message);
                    var json = JsonSerializer.Serialize(response,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    await context.Response.WriteAsync(json);
                },

                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.Fail(
                        "You do not have permission to access this resource. " +
                        "Required role: Admin.");

                    var json = JsonSerializer.Serialize(response,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    await context.Response.WriteAsync(json);
                }
            };
        });

        //  Authorization Policies 
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
            options.AddPolicy("AdminOrCustomer", policy => policy.RequireRole("Admin", "Customer"));

            // Global fallback: every endpoint requires a valid JWT unless [AllowAnonymous] is set
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        //  Swagger / OpenAPI 
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ShopEZ E-Commerce API",
                Version = "v1",
                Description = "RESTful backend for ShopEZ. Manage products, process orders, user auth.",
                Contact = new OpenApiContact { Name = "ShopEZ Team", Email = "support@shopez.com" }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
        });

        //  Build 
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //db.Database.Migrate();
        }

        //  Middleware pipeline 
        app.UseGlobalExceptionHandler();    // 1. Catch all unhandled exceptions

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopEZ API v1");
                c.RoutePrefix = string.Empty;
                c.DocumentTitle = "ShopEZ API";
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("ShopEZCors");
        app.UseAuthentication();            // 2. Identify who is calling
        app.UseAuthorization();             // 3. Check what they're allowed to do
        app.MapControllers();

        app.Run();
    }
}