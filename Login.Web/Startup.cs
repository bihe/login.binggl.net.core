﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Login.Core.Configuration;
using Login.Core.Data;
using Login.Core.Middleware;
using Login.Core.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;

namespace Login.Web
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
                    OnTokenValidated = PerformPostTokenValidationAuthorization,
                    OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
                };
            });

            // Add framework services.
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = ApplicationDescription, Version = "v1" });
                var filePath = System.IO.Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Login.Web.xml");
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

            if (env.IsDevelopment())
            {
                ContextInitializer.InitialData(context);
            }
        }
    }
}
