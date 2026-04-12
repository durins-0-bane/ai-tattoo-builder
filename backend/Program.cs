using Microsoft.Azure.Cosmos;
using TattooShop.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Cosmos DB settings
var cosmosEndpoint = builder.Configuration["CosmosDb:AccountEndpoint"];
var cosmosKey = builder.Configuration["CosmosDb:AccountKey"];
var databaseName = builder.Configuration["CosmosDb:DatabaseName"];

builder.Services.AddSingleton(sp =>
{
    return new CosmosClient(cosmosEndpoint, cosmosKey, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});
builder.Services.AddScoped<IAppointmentRepository, CosmosAppointmentRepository>();
builder.Services.AddScoped<ITattooDesignRepository, CosmosTattooDesignRepository>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { 
                message = "Something went wrong on our end. Please try again later." 
            });
        });
    });
}

app.UseHttpsRedirection();
app.MapControllers();

// Verifies DB Connection
app.MapGet("/api/init-db", async (CosmosClient client) =>
{
    var databaseName = builder.Configuration["CosmosDb:DatabaseName"];
    DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    
    await database.Database.CreateContainerIfNotExistsAsync("Appointments", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("Designs", "/partitionKey");
    
    return Results.Ok(new { message = "Database and Containers initialized!" });
})
.WithName("InitializeDatabase")
.WithOpenApi();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
