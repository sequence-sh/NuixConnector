using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuixFrontEnd.Shared;
using Razor_Components;
using TG.Blazor.IndexedDB;

namespace NuixFrontEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ProcessContext>();

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddIndexedDB(dbStore =>
            {
                dbStore.DbName = Datastore.DbName; 
                dbStore.Version = 1;

                dbStore.Stores.Add(new StoreSchema
                {
                    Name = nameof(MethodMetadata),
                    PrimaryKey = new IndexSpec { Name = nameof(MethodMetadata.Id), KeyPath = nameof(MethodMetadata.Id), Auto = true },
                    Indexes = new List<IndexSpec>
                    {
                        new IndexSpec{Name=nameof(MethodMetadata.ClassAndMethod), KeyPath = nameof(MethodMetadata.ClassAndMethod), Auto=false},
                    }

                });
                
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = nameof(ParameterMetadata),
                    PrimaryKey = new IndexSpec { Name = nameof(ParameterMetadata.Id), KeyPath = nameof(ParameterMetadata.Id), Auto = true },
                    Indexes = new List<IndexSpec>
                    {
                        new IndexSpec{Name=nameof(ParameterMetadata.PClassAndMethod), KeyPath = nameof(ParameterMetadata.PClassAndMethod), Auto=false},
                    }
                });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
