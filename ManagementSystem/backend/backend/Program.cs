using backend.Configuration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);


// =========================
// Add Services
// =========================

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read MongoDB settings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));


// Register MongoClient
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration
        .GetSection("MongoDB")
        .Get<MongoDbSettings>()!;

    return new MongoClient(settings.ConnectionString);
});



var app = builder.Build();


// =========================
// Check MongoDB Connection
// =========================

using (var scope = app.Services.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<IMongoClient>();

    try
    {
        var databases = client.ListDatabaseNames().ToList();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=======================================");
        Console.WriteLine("✅ MongoDB Connected Successfully");
        Console.WriteLine("=======================================");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("=======================================");
        Console.WriteLine("❌ MongoDB Connection Failed");
        Console.WriteLine(ex.Message);
        Console.WriteLine("=======================================");
        Console.ResetColor();
    }
}



// =========================
// Configure Middleware
// =========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();