using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Inyección de la dependencia de conexión de la Base de datos
builder.Services.AddDbContext<DataContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Comportamiento de la contraseña de usuario
//TODO: Make stronger password
builder.Services.AddIdentity<User, IdentityRole>(cfg =>
{
    cfg.User.RequireUniqueEmail = true;
    cfg.Password.RequireDigit = false;
    cfg.Password.RequiredUniqueChars = 0;
    cfg.Password.RequireLowercase = false;
    cfg.Password.RequireNonAlphanumeric = false;
    cfg.Password.RequireUppercase = false;
    cfg.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<DataContext>();

//Redirección de páginas de error
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/NotAuthorized";
    options.AccessDeniedPath = "/Account/NotAuthorized";
});


//Inyección del feedDb 
builder.Services.AddTransient<SeedDb>();

//Inyección de la interface IUserHelper
builder.Services.AddScoped<IUserHelper, UserHelper>();

//Inyección del Combos Helper
builder.Services.AddScoped<ICombosHelper, CombosHelper>();


//Agregar para que actualice los cambios al momento del desarrollo
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

//Llamar al feedDb
SeedData();

void SeedData()
{
    IServiceScopeFactory? scopeFactory = app.Services.GetService<IServiceScopeFactory>();

    using(IServiceScope? scope = scopeFactory.CreateScope())
    {
        SeedDb? service = scope.ServiceProvider.GetService<SeedDb?>();
        service.SeedAsync().Wait();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();//Para usar autenticación en el logIn debe estar antes de app.UseAuthorization();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/error/{0}");//Dirección de la página de error


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
