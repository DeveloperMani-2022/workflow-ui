using WorkflowEngine.Data;
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

// Register SQL Connection Factory
builder.Services.AddSingleton<SqlConnectionFactory>();

// Register Database Initializer
builder.Services.AddScoped<DatabaseInitializer>();

// Register Repositories with interfaces
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IWorkflowVersionRepository, WorkflowVersionRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register services with interfaces
builder.Services.AddScoped<IWorkflowPublisherService, WorkflowPublisherService>();

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
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();
}

app.Run();
