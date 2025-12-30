using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Chirp.Core.Models;
using Chirp.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=Chat.db"; // fallback
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(conn));

builder.Services.AddDefaultIdentity<Author>(options => options.SignIn.RequireConfirmedAccount = true)
.AddEntityFrameworkStores<ChatDBContext>();


builder.Services.AddAuthentication()
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientID"];
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        options.CallbackPath = "/signin-github"; 
    });

builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ChatDBContext>();
    var userManager = services.GetRequiredService<UserManager<Author>>();

    var connectionString = context.Database.GetConnectionString() ?? "";
    if (!connectionString.Contains(":memory:", StringComparison.OrdinalIgnoreCase))
    {
        context.Database.Migrate();
        context.Database.EnsureCreated();

        DbInitializer.SeedDatabase(context, userManager);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapPost("/test/create-user", async (UserManager<Author> userManager) =>
    {
        var testUser = await userManager.FindByEmailAsync("test@chirp.dk");
        if (testUser == null)
        {
            testUser = new Author
            {
                UserName = "test@chirp.dk",
                Email = "test@chirp.dk",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(testUser, "Test123!");
        }
        return Results.Ok();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program {}
