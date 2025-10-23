using Consilient.Data;
using Consilient.WebApp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DevConnection") ?? throw new ArgumentException("connectionString");
builder.Services.RegisterConsilientDataServices(connectionString);

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddSession();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

//// not needed
//builder.Services.Configure<OpenIdConnectOptions>(
//    OpenIdConnectDefaults.AuthenticationScheme, options =>
//    {
//        options.Events = new OpenIdConnectEvents
//        {
//            OnTokenValidated = ctx =>
//            {
//                Console.WriteLine("OIDC Token validated for: " + ctx.Principal.Identity?.Name);
//                return Task.CompletedTask;
//            },
//            OnAuthenticationFailed = ctx =>
//            {
//                Console.WriteLine("OIDC Auth failed: " + ctx.Exception.Message);
//                return Task.CompletedTask;
//            }
//        };
//    });


builder.Services.AddAuthorization();

builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

builder.Services.AddControllersWithViews();

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
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
