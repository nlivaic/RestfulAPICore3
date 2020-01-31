using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using API.Helpers;
using API.Models;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.IO;

namespace RestfulAPICore3.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IValidationProblemDetailsFactory, ValidationProblemDetailsFactory>();
            services.AddTransient<IDataShapingService, DataShapingService>();
            services.AddSingleton<IInvalidModelResultFactory, InvalidModelResultFactory>();
            services.AddSingleton<IPagingService, PagingService>();
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddControllers(configure =>
            {
                configure.ReturnHttpNotAcceptable = true;
                configure.CacheProfiles.Add(
                    "240SecondsCacheProfile",
                    new CacheProfile
                    {
                        Duration = 240
                    });
            })
            .AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var unprocessableEntityFactory = serviceProvider.GetService<IInvalidModelResultFactory>();
                options.InvalidModelStateResponseFactory = unprocessableEntityFactory.Create;
            });
            services.AddHttpCacheHeaders(
                expirationOptions =>
                {
                    expirationOptions.MaxAge = 120;
                },
                validationOptions =>
                {
                    validationOptions.MustRevalidate = true;
                }
            );
            // services.AddResponseCaching();
            services.AddDbContext<CourseLibraryContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("CourseLibraryDatabase")));
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(
                    "LibraryOpenAPISpecification",
                    new OpenApiInfo
                    {
                        Title = "Library API",
                        Version = "v1",
                        Description = "This API allows access to authors and their books.",
                        Contact = new OpenApiContact
                        {
                            Name = "My name",
                            Url = new Uri("https://www.somewhere.com")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT",
                            Url = new Uri("https://www.opensource.org/licenses/MIT")
                        },
                        TermsOfService = new Uri("https://www.your-terms-of-service.com")
                    });
                // A workaround for having multiple POST methods on one controller.
                // setupAction.ResolveConflictingActions(r => r.First());
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "RestfulAPICore3.API.xml"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(
                        async context =>
                        {
                            context.Response.StatusCode = 500;
                            await context.Response.WriteAsync("An unexpected fault happened. Please try again later.");
                        }
                    );
                });
            }

            app.UseHttpsRedirection();

            // app.UseResponseCaching();
            // Take care to put .UseSwagger() after Https redirection, so as to prevent Http requests.
            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/LibraryOpenAPISpecification/swagger.json",
                    "Library API"
                );
                setupAction.RoutePrefix = "";   // This removes the /swagger/ from the endpoint Uri.
            });

            app.UseHttpCacheHeaders();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
