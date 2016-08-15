using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test4.Web.Data;
using Test4.Web.Models;
using Test4.Web.Services;
using System.IO;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Test4.Web
{
    public class Startup
    {


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<RouteOptions>(
                options => {
                    options.AppendTrailingSlash = false;
                    options.LowercaseUrls = true;
                }
                );




            // Session variables support
            //services.AddCaching();
            services.AddSession();

            //upgrade to asp.net core - use propercase and not camelcase for json properties
            services.AddMvc()
              .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            ////change default login location from /Account/Login to /Area/Account/Login
            services.Configure<CookieAuthenticationOptions>(options =>
            {
                options.LoginPath = new PathString("/Account/Login");
            });


            services.Configure<IISOptions>(options => { });
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //another hack
            //https://github.com/aspnet/Hosting/issues/416

            if (env.IsDevelopment())
            {
                this.Configure1( app, env, loggerFactory);
            }
            else
            {
                //this.Configure1(app, env, loggerFactory);
                app.Map("/Test4", (app1) => this.Configure1(app1, env, loggerFactory));
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure1(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

     
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

    
            }


            app.UseIdentity();


            app.UseSession();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {

                routes.MapRoute("areaRoute", "{area:exists}/{controller}/{action}");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
