using agents.milla.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow React dev server
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Production: React is served by same server, but allow some flexibility
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Register DealDeskOrchestrator and related services
builder.Services.AddSingleton<NewDealDeskOrchestrator>();
builder.Services.AddSingleton<ConversationManager>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowReactApp");

// Serve static files from wwwroot
app.UseStaticFiles();

// Serve static files from ClientApp/build for the React app
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build")),
    RequestPath = ""
});

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

// Fallback to serve the React app for any non-API routes
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "build"))
});

app.Run();
