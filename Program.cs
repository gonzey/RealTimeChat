using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 10240 * 10240; 
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Add this line for SignalR

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

// Map the SignalR hub and configure options
app.MapHub<ChatHub>("/chatHub", options =>
{
    options.ApplicationMaxBufferSize = 102400000; // Set the maximum message size (in bytes)
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();
