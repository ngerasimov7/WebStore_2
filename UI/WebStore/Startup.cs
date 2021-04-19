using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WebStore.DAL.Context;
using WebStore.Services.Data;
using WebStore.Domain.Entities.Identity;
using WebStore.Infrastructure.Conventions;
using WebStore.Services;
using WebStore.Services.InCookies;
using WebStore.Services.InMemory;
using WebStore.Services.InSQL;
using WebStore.Interfaces;

namespace WebStore
{
    public record Startup(IConfiguration Configuration)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<WebStoreDB>(opt =>
            //    opt.UseSqlServer(Configuration.GetConnectionString("Default"))
            //    //.EnableSensitiveDataLogging(true)
            //    //.LogTo(Console.WriteLine)
            //    );
            services.AddDbContext<WebStoreDB>(opt => 
                opt.UseSqlite(
                    Configuration.GetConnectionString("Sqlite"), 
                    o => o.MigrationsAssembly("WebStore.DAL.Sqlite")));
            services.AddTransient<WebStoreDbInitializer>();

            services.AddIdentity<User, Role>()
               .AddEntityFrameworkStores<WebStoreDB>()
               .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(opt =>
            {
//#if DEBUG
                opt.Password.RequiredLength = 3;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredUniqueChars = 3;
//#endif
                opt.User.RequireUniqueEmail = false;
                opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

                opt.Lockout.AllowedForNewUsers = false;
                opt.Lockout.MaxFailedAccessAttempts = 10;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.Cookie.Name = "GB.WebStore";
                opt.Cookie.HttpOnly = true;
                opt.ExpireTimeSpan = TimeSpan.FromDays(10);

                opt.LoginPath = "/Account/Login";
                opt.LogoutPath = "/Account/Logout";
                opt.AccessDeniedPath = "/Account/AccessDenied";

                opt.SlidingExpiration = true;
            });

            services.AddTransient<IEmployeesData, InMemoryEmployeesData>();
            //services.AddTransient<IProductData, InMemoryProductData>();
            services.AddScoped<IProductData, SqlProductData>();
            services.AddScoped<ICartServices, InCookiesCartService>();
            services.AddScoped<IOrderService, SqlOrderService>();

            services
               .AddControllersWithViews(
                    mvc =>
                    {
                        mvc.Conventions.Add(new ApplicationConvention());
                    })
               .AddRazorRuntimeCompilation();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebStoreDbInitializer db)
        {
            db.Initialize();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/Greetings", async context =>
                {
                    await context.Response.WriteAsync(Configuration["Greetings"]);
                });

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
