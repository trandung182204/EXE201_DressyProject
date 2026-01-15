using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Add ApplicationDbContext with SQL Server
builder.Services.AddDbContext<BE.Data.ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository and service layers (to be implemented)
builder.Services.AddScoped<BE.Repositories.Interfaces.IProductsRepository, BE.Repositories.Implementations.ProductsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.ICategoriesRepository, BE.Repositories.Implementations.CategoriesRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.ICartsRepository, BE.Repositories.Implementations.CartsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.ICartItemsRepository, BE.Repositories.Implementations.CartItemsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IBookingsRepository, BE.Repositories.Implementations.BookingsRepository>();
builder.Services.AddScoped<BE.Repositories.Interfaces.IProvidersRepository, BE.Repositories.Implementations.ProvidersRepository>();

// Services
builder.Services.AddScoped<BE.Services.Interfaces.IProductsService, BE.Services.Implementations.ProductsService>();
builder.Services.AddScoped<BE.Services.Interfaces.ICategoriesService, BE.Services.Implementations.CategoriesService>();
builder.Services.AddScoped<BE.Services.Interfaces.ICartsService, BE.Services.Implementations.CartsService>();
builder.Services.AddScoped<BE.Services.Interfaces.ICartItemsService, BE.Services.Implementations.CartItemsService>();
builder.Services.AddScoped<BE.Services.Interfaces.IBookingsService, BE.Services.Implementations.BookingsService>();
builder.Services.AddScoped<BE.Services.Interfaces.IProvidersService, BE.Services.Implementations.ProvidersService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://127.0.0.1:5500")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


app.UseCors("AllowFrontend");


app.Run();
