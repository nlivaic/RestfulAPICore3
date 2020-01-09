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
            services.AddResponseCaching();
            services.AddDbContext<CourseLibraryContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("CourseLibraryDatabase")));
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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

            app.UseResponseCaching();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
