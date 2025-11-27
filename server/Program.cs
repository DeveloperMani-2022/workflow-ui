using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Data;
using WorkflowEngine.Models.NodeExecutors;
using WorkflowEngine.Services;
using WorkflowEngine.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep property names as-is
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Use SQL Server if connection string is provided, otherwise use In-Memory for development
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("WorkflowEngineDb");
    }
});

// Register HttpClient for API call nodes
builder.Services.AddHttpClient();

// Register node executors
builder.Services.AddScoped<INodeExecutor, MessageNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, QuestionNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, APICallNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, ConditionNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, LLMNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, StateNodeExecutor>();

// Register services
builder.Services.AddScoped<WorkflowCompilerService>();
builder.Services.AddScoped<WorkflowValidationService>();
builder.Services.AddScoped<WorkflowExecutionService>();
builder.Services.AddScoped<WorkflowPublisherService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React dev servers
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Workflow Engine API",
        Version = "v1",
        Description = "API for managing and executing agentic AI workflows"
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Engine API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Use error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // For in-memory database, ensure it's created
    if (dbContext.Database.IsInMemory())
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        // For SQL Server, apply migrations
        dbContext.Database.Migrate();
    }
}

app.Run();
