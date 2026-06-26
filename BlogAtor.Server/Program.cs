using Microsoft.EntityFrameworkCore;
using BlogAtor.Server.Config;
using BlogAtor.Server.Data;
using BlogAtor.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка конфигурации Reddit
builder.Services.Configure<RedditConfig>(
    builder.Configuration.GetSection("Reddit"));

// 2. Настройка DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Регистрация сервисов
builder.Services.AddScoped<IRedditService, RedditService>();

// 4. Добавление контроллеров
builder.Services.AddControllers();

// 5. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BlogAtor API",
        Version = "v1",
        Description = "API для сбора новостей из социальных сетей"
    });
});

// 6. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// 7. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogAtor API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// 8. Создание БД
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();