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

// Razor
builder.Services.AddRazorPages();

// EF Core: read connection string and register DbContext
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=Chat.db"; // fallback
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(conn));

builder.Services.AddDefaultIdentity<ApplicationAuthor>(options => options.SignIn.RequireConfirmedAccount = true)
.AddEntityFrameworkStores<ChatDBContext>();

// Your service (scoped is typical since DbContext is scoped)
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ChatDBContext>();
    context.Database.EnsureCreated();
    DbInitializer.SeedDatabase(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
