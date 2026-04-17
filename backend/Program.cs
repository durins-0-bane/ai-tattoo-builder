using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using TattooShop.Api.Features.Behaviors;
using TattooShop.Api.Repositories;
using TattooShop.Api.Services;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from Azure App Configuration (if configured)
var appConfigConnectionString = builder.Configuration["AppConfig:ConnectionString"];
var appConfigEndpoint = builder.Configuration["AppConfig:Endpoint"];

if (!string.IsNullOrWhiteSpace(appConfigConnectionString))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(appConfigConnectionString)
            .Select(KeyFilter.Any, LabelFilter.Null);
    });
}
else if (!string.IsNullOrWhiteSpace(appConfigEndpoint))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
            .Select(KeyFilter.Any, LabelFilter.Null);
    });
}

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
builder.Services.AddScoped<IArtistProfileRepository, CosmosArtistProfileRepository>();
builder.Services.AddScoped<IChatMessageRepository, CosmosChatMessageRepository>();
builder.Services.AddScoped<IChatSessionRepository, CosmosChatSessionRepository>();
builder.Services.AddScoped<ITattooDesignRepository, CosmosTattooDesignRepository>();
builder.Services.AddScoped<IUserRepository, CosmosUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<ChatExecutionContext>();
builder.Services.AddScoped<ITattooAgentService, TattooAgentService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
builder.Services.AddHttpClient();

var jwtSecret = builder.Configuration["Auth:Jwt:Secret"]
    ?? throw new InvalidOperationException("Auth:Jwt:Secret is not configured.");
var jwtIssuer = builder.Configuration["Auth:Jwt:Issuer"] ?? "TattooShop.Api";
var jwtAudience = builder.Configuration["Auth:Jwt:Audience"] ?? "TattooShop.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

var openAiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");
var openAiModel = builder.Configuration["OpenAI:ModelId"] ?? throw new InvalidOperationException("OpenAI:ModelId is not configured");

builder.Services.AddKernel()
    .AddOpenAIChatCompletion(openAiModel, openAiKey);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder.WithOrigins(
                        "http://localhost:7247",
                        "http://localhost:5173",
                        "https://durins-0-bane.github.io")
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed artist profiles on startup
using (var scope = app.Services.CreateScope())
{
    var artistRepo = scope.ServiceProvider.GetRequiredService<IArtistProfileRepository>();
    await artistRepo.EnsureSeedDataAsync();
}

// Verifies DB Connection
app.MapGet("/api/init-db", async (CosmosClient client) =>
{
    DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    
    await database.Database.CreateContainerIfNotExistsAsync("Appointments", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("Artists", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("ChatMessages", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("ChatSessions", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("Designs", "/partitionKey");
    await database.Database.CreateContainerIfNotExistsAsync("Users", "/partitionKey");
    
    return Results.Ok(new { message = "Database and Containers initialized!" });
})
.WithName("InitializeDatabase")
.WithOpenApi();

app.Run();