var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Enable serving static files (for the ES6 frontend)
app.UseDefaultFiles();
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Map MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
