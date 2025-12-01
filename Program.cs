using SwiftOpsToolbox.Components;
using SwiftOpsToolbox.Services;
using SwiftOpsToolbox.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Google Calendar settings
builder.Services.Configure<GoogleCalendarConfig>(
    builder.Configuration.GetSection("GoogleCalendar"));

// Add Google Calendar Services
builder.Services.AddSingleton<GoogleCalendarAuthService>();
builder.Services.AddSingleton<GoogleCalendarIntegrationService>();

// Add Calendar Service
builder.Services.AddSingleton<CalendarService>();

// Add Holiday Service
builder.Services.AddSingleton<HolidayService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
