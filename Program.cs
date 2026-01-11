using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VistaTiBooks.Api.Data;
using VistaTiBooks.Api.Services; // <-- si tu namespace es ese

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON options
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// DB Context (lo tuyo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient para Open Library (NUEVO)
builder.Services.AddHttpClient<OpenLibraryService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org");
});

// Swagger (lo tuyo)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
