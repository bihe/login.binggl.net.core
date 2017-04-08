using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Login.Core.Configuration;
using Login.Core.Data;
using Login.Core.Middleware;
using Login.Core.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Login.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if(env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets("941c25ef-e985-4331-86ca-066139a141b9");
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // add configuration sections
            services.Configure<ApplicationConfiguration>(Configuration.GetSection("Application"));

            // add database services
            services.AddDbContext<LoginContext>(options => options.UseMySql(Configuration.GetConnectionString("LoginConnection")));

            services.AddMemoryCache();

            services.AddSingleton<IAuthorization, DatabaseAuthorization>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddSingleton<IFlashService, MemoryFlashService>();
            services.AddSingleton<IMessageIntegrity, HashedMessageIntegrity>();

            // Add framework services.
            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IAuthorization auth, IOptions<ApplicationConfiguration> appConfig, LoginContext context,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddSerilog();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            var googleClientId = "";
            var googleClientSecret = "";

            if (env.IsDevelopment())
            {
                app.UseErrorHandling();
                //app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                googleClientId =  Configuration["GoogleClientId"];
                googleClientSecret = Configuration["GoogleClientSecret"];
            }
            else
            {
                googleClientId = appConfig.Value.Authentication.GoogleClientId;
                googleClientSecret = appConfig.Value.Authentication.GoogleClientSecret;

                app.UseErrorHandling();
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = Constants.AUTH_SCHEME,
                CookieName = appConfig.Value.Authentication.CookieName,
                AutomaticAuthenticate = false,
                CookieSecure = env.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = Constants.AUTH_SCHEME,
                AutomaticAuthenticate = true,
                Authority = "https://accounts.google.com",
                ResponseType = "code id_token",
                ClientId = googleClientId,
                ClientSecret = googleClientSecret,
                GetClaimsFromUserInfoEndpoint = true,
                Scope = {"openid", "profile", "email"},
                SaveTokens = true,
                Events = new OpenIdConnectEvents
                {
                    //OnAuthenticationFailed = OnAuthenticationFailed,
                    //OnRemoteFailure = OnRemoteFailure
                    OnTokenValidated = auth.PerformPostTokenValidationAuthorization
                },
            });

            app.UseJwtProcessor();

            var cultures = new List<CultureInfo> { new CultureInfo("en-US"), new CultureInfo("en"), new CultureInfo("de-DE"), new CultureInfo("de") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = cultures,
                // UI strings that we have localized.
                SupportedUICultures = cultures
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            if(env.IsDevelopment())
            {
                ContextInitializer.InitialData(context);
            }
         }
    }
}
