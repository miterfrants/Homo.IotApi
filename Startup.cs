using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Homo.AuthApi;
using Homo.Api;

namespace Homo.IotApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            Console.WriteLine(env.EnvironmentName);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"secrets.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        readonly string AllowSpecificOrigins = "";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Homo.AuthApi.AppSettings appSettings = new Homo.AuthApi.AppSettings();
            Configuration.GetSection("Config").Bind(appSettings);
            services.Configure<Homo.AuthApi.AppSettings>(Configuration.GetSection("Config"));

            // setup CROS if config file includ CROS section
            IConfigurationSection CROSSection = Configuration.GetSection("CROS");

            string stringCrossOrigins = Configuration.GetSection("CROS").GetValue<string>("Origin");
            if (stringCrossOrigins != null)
            {
                string[] crossOrigins = stringCrossOrigins.Split(",");
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(crossOrigins));

                services.AddCors(options =>
                {
                    options.AddPolicy(AllowSpecificOrigins,
                        builder =>
                        {
                            builder.WithOrigins(crossOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
                        });
                });
            }
            CultureInfo currentCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            services.AddSingleton<ErrorMessageLocalizer>(new ErrorMessageLocalizer(appSettings.Common.LocalizationResourcesPath));
            services.AddSingleton<CommonLocalizer>(new CommonLocalizer(appSettings.Common.LocalizationResourcesPath));
            services.AddSingleton<ValidationLocalizer>(new ValidationLocalizer(appSettings.Common.LocalizationResourcesPath));
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            var secrets = (Homo.AuthApi.Secrets)appSettings.Secrets;
            services.AddDbContext<IotDbContext>(options => options.UseMySql(secrets.DBConnectionString, serverVersion));
            services.AddDbContext<Homo.AuthApi.DBContext>(options => options.UseMySql(secrets.DBConnectionString, serverVersion));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api Doc", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "dev")
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "homo auth api v1"));
            }

            var supportedCultures = new[] {
                new CultureInfo("zh-TW"),
                new CultureInfo("en-US"),
                new CultureInfo("fr"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("zh-TW"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider{QueryStringKey= "culture"}
                }
            });

            String apiPrefix = Configuration.GetSection("Config").GetSection("Common").GetValue<string>("ApiPrefix");
            app.UseCors(AllowSpecificOrigins);
            app.UseHttpsRedirection();
            app.UsePathBase(new PathString($"/{apiPrefix}"));
            app.UseRouting();
            app.UseMiddleware(typeof(AuthApiErrorHandlingMiddleware));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}