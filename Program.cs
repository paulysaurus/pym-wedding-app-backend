using MongoDB.Driver;
using WeddingApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoConnectionString = builder.Configuration["MongoDb:ConnectionString"]
    ?? throw new InvalidOperationException("No se configuró MongoDb:ConnectionString.");
var mongoDatabaseName = builder.Configuration["MongoDb:DatabaseName"]
    ?? throw new InvalidOperationException("No se configuró MongoDb:DatabaseName.");
var mongoCollectionName = builder.Configuration["MongoDb:CollectionName"] ?? "GuestRsvps";

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);
builder.Services.AddSingleton(mongoDatabase.GetCollection<GuestRsvp>(mongoCollectionName));

var allowedOrigins = builder.Configuration
    .GetSection("Frontend:Urls")
    .Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
