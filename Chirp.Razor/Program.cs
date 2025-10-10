using Chirp.Razor.Models;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Razor
builder.Services.AddRazorPages();

// EF Core: read connection string and register DbContext
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=Chat.db"; // fallback
builder.Services.AddDbContext<ChatDBContext>(options => options.UseSqlite(conn));

// Your service (scoped is typical since DbContext is scoped)
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

var app = builder.Build();

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

app.MapRazorPages();

app.Run();
