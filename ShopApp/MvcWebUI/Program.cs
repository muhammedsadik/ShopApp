using Business.Abstract;
using Business.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EfCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MvcWebUI.EmailServices;
using MvcWebUI.Identity;
using Entity;
using MvcWebUI.Extensions;

var builder = WebApplication.CreateBuilder(args);

//For Sqlite 
//builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

//builder.Services.AddDbContext<ShopContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection")));

builder.Services.AddDbContext<ShopContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection")));



builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();


//Identity ayarlar�
builder.Services.Configure<IdentityOptions>(options =>
{
  //Password i�lemleri i�in validation tan�mlamalar�
  options.Password.RequireLowercase = true;
  options.Password.RequireUppercase = true;

  //LockOut i�lemleri
  options.Lockout.AllowedForNewUsers = true;//lockout aktif edildi
  options.Lockout.MaxFailedAccessAttempts = 5;//max 5 defa yanl�� �ifre girme

  //User i�lemleri
  options.User.RequireUniqueEmail = true;//her kullan�c�n�n mailleri farkl�
  options.SignIn.RequireConfirmedEmail = true;
});

//Cookie i�lemleri
builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/account/login"; //uyglam cookie tan�m�yorsa y�nlndirr.
  options.LogoutPath = "/account/logout";//��k�� yap�nca buraya y�nlendirir.
  options.AccessDeniedPath = "/account/accessdenied";//login ama y�netici de�il
  options.SlidingExpiration = true;//loginden sonra 20dk ��k�� verir
  options.ExpireTimeSpan = TimeSpan.FromDays(365);//login s�re uzat�ld�(g�n)
  options.Cookie = new CookieBuilder
  {
    HttpOnly = true,
    Name = ".ShopApp.Security.Cookie",
    SameSite = SameSiteMode.Strict
  };

});


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICartService, CartManager>();
builder.Services.AddScoped<IOrderService, OrderManager>();

//Email lerin ayarlamalar�n� buradan 572.video
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>(i =>
    new SmtpEmailSender(
      builder.Configuration["EmailSender:Host"],
      builder.Configuration.GetValue<int>("EmailSender:Port"),
      builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
      builder.Configuration["EmailSender:UserName"],
      builder.Configuration["EmailSender:Password"]
      ));

builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();

}


app.MigrateDatabase();
app.UseHttpsRedirection();
app.UseStaticFiles();

//extra dosyay� tan�mlamad�k
app.UseStaticFiles(new StaticFileOptions
{
  FileProvider = new PhysicalFileProvider(
      Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
    ),
  RequestPath = "/wwwroot"
});


app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

#region Routes

app.MapControllerRoute(
    name: "orders",
    pattern: "orders",
    defaults: new { controller = "Cart", action = "GetOrders" }
    );

app.MapControllerRoute(
    name: "checkout",
    pattern: "checkout",
    defaults: new { controller = "Cart", action = "Checkout" }
    );

app.MapControllerRoute(
    name: "cart",
    pattern: "cart",
    defaults: new { controller = "Cart", action = "Index" }
    );

app.MapControllerRoute(
    name: "adminusers",
    pattern: "admin/user/list",
    defaults: new { controller = "Admin", action = "UserList" }
    );

app.MapControllerRoute(
    name: "adminuseredit",
    pattern: "admin/user/Delete",
    defaults: new { controller = "Admin", action = "UserDelete" }
    );

app.MapControllerRoute(
    name: "adminuseredit",
    pattern: "admin/user/{id?}",
    defaults: new { controller = "Admin", action = "UserEdit" }
    );

app.MapControllerRoute(
    name: "adminroles",
    pattern: "admin/role/list",
    defaults: new { controller = "Admin", action = "RoleList" }
    );

app.MapControllerRoute(
    name: "adminroledelete",
    pattern: "admin/role/delete",
    defaults: new { controller = "Admin", action = "RoleDelete" }
    );

app.MapControllerRoute(
    name: "adminrolecreate",
    pattern: "admin/role/create",
    defaults: new { controller = "Admin", action = "RoleCreate" }
    );

app.MapControllerRoute(
    name: "adminroleedit",
    pattern: "admin/role/{id?}",
    defaults: new { controller = "Admin", action = "RoleEdit" }
    );

app.MapControllerRoute(
    name: "adminproducts",
    pattern: "admin/products",
    defaults: new { controller = "Admin", action = "ProductList" }
    );

app.MapControllerRoute(
    name: "admincateproduct",
    pattern: "admin/products/create",
    defaults: new { controller = "Admin", action = "ProductCreate" }
    );

app.MapControllerRoute(//1
    name: "adminproductedit",
    pattern: "admin/products/{id?}",
    defaults: new { controller = "Admin", action = "ProductEdit" }
    );

app.MapControllerRoute(
    name: "admincategories",
    pattern: "admin/categories",
    defaults: new { controller = "Admin", action = "CategoryList" }
    );

app.MapControllerRoute(
    name: "admincategorycreate",
    pattern: "admin/categories/create",
    defaults: new { controller = "Admin", action = "CategoryCreate" }
    );

app.MapControllerRoute(
    name: "admincategoryedit",
    pattern: "admin/categories/{id?}",
    defaults: new { controller = "Admin", action = "CategoryEdit" }
    );

app.MapControllerRoute(
    name: "search",
    pattern: "search",
    defaults: new { controller = "Shop", action = "Search" }
    );

app.MapControllerRoute(
    name: "about",
    pattern: "about",
    defaults: new { controller = "Shop", action = "About" }
    );

app.MapControllerRoute(
    name: "products",
    pattern: "products/{category?}",
    defaults: new { controller = "Shop", action = "List" }
    );

app.MapControllerRoute(
    name: "productsdetails",
    pattern: "{url}",
    defaults: new { controller = "Shop", action = "Details" }
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );
#endregion


using (var scope = app.Services.CreateScope())
{
  var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
  var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
  var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
  var configuration =scope.ServiceProvider.GetRequiredService<IConfiguration>();

  SeedIdentity.Seed(userManager, roleManager, cartService, configuration).Wait();
}


app.Run();