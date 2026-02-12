using System.Text;
using System.Text.Json;
using BE.Data;
using BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
});
// Repositories
builder.Services.AddScoped<BE.Repositories.Interfaces.IProductsRepository, BE.Repositories.Implementations.ProductsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.ICategoriesRepository, BE.Repositories.Implementations.CategoriesRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.ICartsRepository, BE.Repositories.Implementations.CartsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.ICartItemsRepository, BE.Repositories.Implementations.CartItemsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IBookingsRepository, BE.Repositories.Implementations.BookingsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IUsersRepository, BE.Repositories.Implementations.UsersRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IFeedbackResponsesRepository, BE.Repositories.Implementations.FeedbackResponsesRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IPaymentsRepository, BE.Repositories.Implementations.PaymentsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IProvidersRepository, BE.Repositories.Implementations.ProvidersRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IProviderBranchesRepository, BE.Repositories.Implementations.ProviderBranchesRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IProductsCustomerRepository, BE.Repositories.Implementations.ProductsCustomerRepository>();

// Services
builder.Services.AddScoped<BE.Services.Interfaces.IProductsService, BE.Services.Implementations.ProductsService>();
builder.Services.AddScoped<BE.Services.Interfaces.ICategoriesService, BE.Services.Implementations.CategoriesService>();
builder.Services.AddScoped<BE.Services.Interfaces.ICartsService, BE.Services.Implementations.CartsService>();
builder.Services.AddScoped<BE.Services.Interfaces.ICartItemsService, BE.Services.Implementations.CartItemsService>();
builder.Services.AddScoped<BE.Services.Interfaces.IBookingsService, BE.Services.Implementations.BookingsService>();
builder.Services.AddScoped<BE.Services.Interfaces.IUsersService, BE.Services.Implementations.UsersService>();
builder.Services.AddScoped<BE.Services.Interfaces.IFeedbackResponsesService, BE.Services.Implementations.FeedbackResponsesService>();
builder.Services.AddScoped<BE.Services.Interfaces.IPaymentsService, BE.Services.Implementations.PaymentsService>();
builder.Services.AddScoped<BE.Services.Interfaces.IProvidersService, BE.Services.Implementations.ProvidersService>();
builder.Services.AddScoped<BE.Services.Interfaces.IProviderBranchesService, BE.Services.Implementations.ProviderBranchesService>();
builder.Services.AddScoped<BE.Services.Interfaces.IProductsCustomerService, BE.Services.Implementations.ProductsCustomerService>();

// AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// CORS (giữ đúng policy bạn đang dùng)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://127.0.0.1:5500") ||
                origin.StartsWith("http://localhost:5500")  ||
                origin.StartsWith("http://127.0.0.1:5501") ||
                origin.StartsWith("http://localhost:5501")
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.IncludeErrorDetails = true;

        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),

            // ✅ map claim
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,

            ClockSkew = TimeSpan.Zero
        };

        // ✅ DEBUG JWT: xem Authorization header, token parse, claims, lỗi validate
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var auth = context.Request.Headers["Authorization"].ToString();

                Console.WriteLine("==================================================");
                Console.WriteLine("[JWT] OnMessageReceived");
                Console.WriteLine($"Path: {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}");
                Console.WriteLine($"Authorization header RAW: {auth}");

                // Token mà JwtBearer sẽ dùng (sau khi parse "Bearer ...")
                Console.WriteLine($"context.Token BEFORE: {context.Token}");

                // Một số trường hợp Postman/Proxy chèn thêm header kỳ lạ
                var proxyAuth = context.Request.Headers["Proxy-Authorization"].ToString();
                if (!string.IsNullOrWhiteSpace(proxyAuth))
                    Console.WriteLine($"Proxy-Authorization RAW: {proxyAuth}");

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("[JWT] OnTokenValidated: ✅ VALID");

                var claims = context.Principal?.Claims
                    .Select(c => $"{c.Type} = {c.Value}")
                    .ToList() ?? new List<string>();

                Console.WriteLine("Claims:");
                foreach (var c in claims) Console.WriteLine("  " + c);

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("[JWT] OnAuthenticationFailed: ❌ FAILED");
                Console.WriteLine(context.Exception.ToString());

                // Giữ header ngắn gọn để bạn vẫn nhìn thấy trên Postman
                context.Response.Headers["jwt-error"] = context.Exception.GetType().Name;

                var msg = (context.Exception.Message ?? "")
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .Trim();

                if (msg.Length > 200) msg = msg.Substring(0, 200);
                context.Response.Headers["jwt-error-message"] = msg;

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Console.WriteLine("[JWT] OnChallenge");
                Console.WriteLine($"Error: {context.Error}");
                Console.WriteLine($"ErrorDescription: {context.ErrorDescription}");

                if (!string.IsNullOrEmpty(context.Error))
                    context.Response.Headers["jwt-error"] = context.Error.Replace("\r", " ").Replace("\n", " ").Trim();

                if (!string.IsNullOrEmpty(context.ErrorDescription))
                {
                    var d = context.ErrorDescription.Replace("\r", " ").Replace("\n", " ").Trim();
                    if (d.Length > 200) d = d.Substring(0, 200);
                    context.Response.Headers["jwt-error-message"] = d;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await DbSeeder.SeedAdminAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseCors("AllowFrontend");


app.UseStaticFiles();


var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();