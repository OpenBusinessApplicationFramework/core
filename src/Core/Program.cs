using Core.Db;
using Core.Filter;
using Core.Services.Action;
using Core.Services.Common;
using Core.Services.Data;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequireSelectFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddOData(options => options.Select().OrderBy().Filter().SetMaxTop(100))
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration["DbConnectionStrings"] ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? throw new Exception("'DB_CONNECTION_STRING' not set"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        }
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["UIFrontendURI"] ?? Environment.GetEnvironmentVariable("UI_FRONTEND_URI") ?? throw new Exception("'UI_FRONTEND_URI' not set"))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<CommonService>();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<DataAnnotationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
