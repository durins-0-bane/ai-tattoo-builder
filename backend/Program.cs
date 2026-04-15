using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using TattooShop.Api.Features.Behaviors;
using TattooShop.Api.Repositories;
using TattooShop.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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
builder.Services.AddScoped<ITattooAgentService, TattooAgentService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
builder.Services.AddHttpClient();

var openAiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");
var openAiModel = builder.Configuration["OpenAI:ModelId"] ?? throw new InvalidOperationException("OpenAI:ModelId is not configured");

builder.Services.AddKernel()
    .AddOpenAIChatCompletion(openAiModel, openAiKey);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder.WithOrigins("http://localhost:5055", "https://durins-0-bane.github.io")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

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
app.UseCors("AllowFrontend");
app.MapControllers();

// Verifies DB Connection
app.MapGet("/api/init-db", async (CosmosClient client) =>
{
    DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    
    await database.Database.CreateContainerIfNotExistsAsync("Appointments", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("Designs", "/partitionKey");
    
    return Results.Ok(new { message = "Database and Containers initialized!" });
})
.WithName("InitializeDatabase")
.WithOpenApi();

app.Run();