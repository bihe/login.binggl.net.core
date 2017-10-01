using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Login.Core.Configuration;
using Login.Core.Data;
using Login.Core.Middleware;
using Login.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;

namespace Login.Web
{
    public class Startup
    {
        const string SwaggerApiDescription = "login.binggl.net API";

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(builder.Build())
               .CreateLogger();


            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets("941c25ef-e985-4331-86ca-066139a141b9");
            }
            CurrentEnvironment = env;
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        private IHostingEnvironment CurrentEnvironment { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<ApplicationConfiguration>(Configuration.GetSection("Application"));

            services.AddMemoryCache();

            services.AddDbContextPool<LoginContext>(options => {
                options.UseMySql(Configuration.GetConnectionString("LoginConnection"));
            });

            services.AddScoped<IAuthorization, DatabaseAuthorization>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddSingleton<IFlashService, MemoryFlashService>();
            services.AddSingleton<IMessageIntegrity, HashedMessageIntegrity>();

            var googleClientId = "";
            var googleClientSecret = "";

            if (CurrentEnvironment.IsDevelopment())
            {
                googleClientId = Configuration["GoogleClientId"];
                googleClientSecret = Configuration["GoogleClientSecret"];
            }
            else
            {
                googleClientId = Configuration["Application:Authentication:GoogleClientId"];
                googleClientSecret = Configuration["Application:Authentication:GoogleClientSecret"];
            }

            // enable authentication; state kept in cookies; using OpenIdConnect - with AAD
            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.Cookie.SecurePolicy = CurrentEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                options.Cookie.Name = Configuration["Application:Authentication:CookieName"];
                options.SlidingExpiration = true;
                options.ReturnUrlParameter = "";
                options.ExpireTimeSpan = TimeSpan.FromDays(double.Parse(Configuration["Application:Authentication:CookieExpiryDays"]));
            })
            .AddOpenIdConnect(options => {
                options.Authority = "https://accounts.google.com";
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.ResponseType = "code id_token";
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("email");
                options.SaveTokens = true;
                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = OnAuthenticationFailed,
                    OnTokenValidated = GetAuthorization(services).PerformPostTokenValidationAuthorization,
                    OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
                };
            });

            // Add framework services.
            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = SwaggerApiDescription, Version = "v1" });
                var filePath = System.IO.Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Login.Web.xml");
                c.IncludeXmlComments(filePath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IAuthorization auth, IOptions<ApplicationConfiguration> appConfig, LoginContext context,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddSerilog();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseErrorHandling();
                app.UseBrowserLink();
            }
            else
            {
                app.UseErrorHandling();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var cultures = new List<CultureInfo> { new CultureInfo("en-US"), new CultureInfo("en"), new CultureInfo("de-DE"), new CultureInfo("de") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = cultures,
                // UI strings that we have localized.
                SupportedUICultures = cultures
            });

            // enable authentication; state kept in cookies; using OpenIdConnect - with AAD
            app.UseAuthentication();

            app.UseJwtProcessor();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // only allow to view swagger if loged in
            app.UseSwaggerAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", SwaggerApiDescription);
            });

            if (env.IsDevelopment())
            {
                ContextInitializer.InitialData(context);
            }
        }

        IAuthorization GetAuthorization(IServiceCollection services)
        {
            IAuthorization authorization = null;
            var scopeFactory = services
                    .BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>();
            
            var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
            authorization = provider.GetRequiredService<IAuthorization>();
            return authorization;
        }

        // Handle sign-in errors differently than generic errors.
        static Task OnAuthenticationFailed(RemoteFailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/error?message=" + context.Failure.Message);
            return Task.FromResult(0);
        }

        // Handle sign-in errors differently than generic errors.
        static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/logoff");
            return Task.FromResult(0);
        }
    }
}