using System;
using System.Collections.Generic;
using System.Globalization;
using Login.Api.Infrastructure.Middleware;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using System.Net.Mime;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;
using Login.Api.Infrastructure.Configuration;
using Login.Api.Features.Shared.Persistence;
using Login.Api.Features.User;
using Login.Api.Infrastructure.FlashScope;
using Login.Api.Infrastructure.Messages;

namespace Login.Api.Infrastructure
{
    public partial class Startup
    {
        const string ApplicationName = "login.binggl.net";
        const string ApplicationDescription = "login.binggl.net API";
        readonly ILogger logger;
        private IServiceCollection _serviceColletion;

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            CurrentEnvironment = env;
            Configuration = configuration;
            this.logger = logger;

            logger.LogInformation($"Started application '{ApplicationName}' in mode '{env.EnvironmentName}'.");
        }

        public IConfiguration Configuration { get; }

        private IHostingEnvironment CurrentEnvironment { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _serviceColletion = services;

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<ApplicationConfiguration>(Configuration.GetSection("Application"));
            services.AddMemoryCache();
                        services.AddDbContextPool<LoginContext>(options => {
                options.UseSqlite(Configuration.GetConnectionString("LoginConnection"));
            });

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

            // enable authentication; state kept in cookies; using OpenIdConnect - with Google
            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.Cookie.SecurePolicy = CurrentEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                options.Cookie.Name = Configuration["Application:Authentication:CookieName"];
                options.Cookie.MaxAge = TimeSpan.FromDays(double.Parse(Configuration["Application:Authentication:CookieExpiryDays"]));
                options.SlidingExpiration = true;
                options.ReturnUrlParameter = "";
                options.AccessDeniedPath = "/error";
                options.LoginPath = "/error";
                options.LogoutPath = "/logout";
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
                    OnTokenValidated = PerformPostTokenValidationAuthorization,
                    OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
                };
            });

            services.Configure<RazorViewEngineOptions>(options => {
                options.ViewLocationExpanders.Add(new FeaturesViewLocationExpander());
            });

#if BLAZOR
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            services.AddResponseCompression(options => {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });
#else
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
#endif

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = ApplicationDescription, Version = "v1" });
                var filePath = System.IO.Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Login.Api.xml");
                c.IncludeXmlComments(filePath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<ApplicationConfiguration> appConfig, LoginContext context,
            IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseErrorHandling();
            }
            else
            {
                app.UseHsts();
                app.UseErrorHandling();
            }

            app.UseSecurityHeaders(options => {
                options.ApplicationBaseUrl = appConfig.Value.BaseUrl;
            });

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
            app.UseHttpsRedirection();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", ApplicationDescription);
            });
#if BLAZOR
            app.UseBlazor<Blazor.Program>();
#endif
            if (env.IsDevelopment())
            {
                ContextInitializer.InitialData(context);
            }
        }
    }
}