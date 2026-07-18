using RealEstateApp.Infrastructure.Identity;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Core.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registro de las capas
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication DEBE ir siempre antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();