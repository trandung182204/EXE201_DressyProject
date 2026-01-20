using System.Text;
using BE.Data;
using BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// AuthService (login/register)
builder.Services.AddScoped<IAuthService, AuthService>();

// Controllers + JSON ignore cycles
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// CORS (giữ đúng policy bạn đang dùng)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thứ tự middleware: CORS -> Auth -> Authorization -> MapControllers
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
