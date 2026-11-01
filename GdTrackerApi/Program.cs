using GdTracker.Core.Repository;
using GdTracker.Core.Service;
using GdTracker.Infra.Data;
using GdTracker.Infra.Repository;
using GdTracker.Infra.Service;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// This reads Render's port, or defaults to 5159 for local dev.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5159";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// 👇 2. CORS CONFIGURATION
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000", // For local testing
            "https://gd-tracker-frontend-daa5-osamify.vercel.app", // 👈 Your EXACT live Vercel URL
            "https://gd-tracker.app", // Your custom domain
            "https://www.gd-tracker.app" // Your custom domain with www
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ILevelRepository, LevelRepository>();
builder.Services.AddScoped<ILevelService, LevelService>();
builder.Services.AddHttpClient(); // This is required for the Service to use HttpClient!
var app = builder.Build();

// 👇 3. ENABLE CORS BEFORE MAPPING CONTROLLERS
app.UseCors("AllowFrontend");

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Note: We removed UseHttpsRedirection because Render handles SSL termination at the proxy level.

app.UseAuthorization();
app.MapControllers();
app.Run();