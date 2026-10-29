using DotNetEd.CoreAdmin;
using DotNetEd.CoreAdmin.DemoApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add the DB Contexts
builder.Services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("TestDatabase"));

// add Core Admin
// Demo-only opt-in. This bypass is effective only in Development.
// Real applications should configure a role restriction or custom authorization.
builder.Services.AddCoreAdmin(new CoreAdminOptions
{
    AllowUnauthenticatedAccessInDevelopment = builder.Environment.IsDevelopment()
});

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

app.UseAuthorization();

app.MapRazorPages();

app.UseCoreAdminCustomTitle("Core Admin Demo App");

// Required for Core Admin
app.MapDefaultControllerRoute();

app.Run();
