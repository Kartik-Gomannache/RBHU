

using RBHU_DbServices.Implementation;
using RBHU_DbServices.Models;
using RBHU_DbServices.Interface;
using Microsoft.EntityFrameworkCore;
using UserManagement.Lib;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RBHUContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

builder.Services.AddTransient<IEService,EService>(provide =>
{
    // Retrieve the connection string from configuration
    var connectionString = builder.Configuration.GetConnectionString("Connection");
    
    // Pass the connection string to the EService constructor
    return new EService(connectionString);
});
builder.Services.AddScoped<IProductService>(provider =>
    new ProductService(builder.Configuration.GetConnectionString("Connection")));

builder.Services.AddUserManagementServices(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "categoryBrand",
    pattern: "Product/Category/{categoryName}/{brandName?}",
    defaults: new { controller = "Product", action = "CategoryByBrand" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.Run();
