using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Extenso.AspNetCore.Mvc.ExtensoUI;
using Extenso.AspNetCore.Mvc.ExtensoUI.Providers;
using Extenso.AspNetCore.Mvc.Rendering;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Queryz.Data;
using Queryz.Data.Domain;
using Queryz.Data.TransformFunctions;
using Queryz.Infrastructure;
using Queryz.Services;

namespace Queryz
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
            services.AddAutofac();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.
            //});

            services.AddOData();

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages();

            services.AddSession();
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseExtensoUI<Bootstrap4UIProvider>();

            app.UseAuthentication();
            app.UseAuthorization();

            // IMPORTANT: This session call MUST go before UseMvc()
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                // Enable all OData functions
                endpoints.Select().Expand().Filter().OrderBy().MaxTop(null).Count();

                var registrars = serviceProvider.GetRequiredService<IEnumerable<IODataRegistrar>>();
                foreach (var registrar in registrars)
                {
                    registrar.Register(endpoints, app.ApplicationServices);
                }

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationDbContextFactory>().As<IDbContextFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<QueryzRazorViewRenderService>().As<IRazorViewRenderService>().SingleInstance();
            builder.RegisterType<ODataRegistrar>().As<IODataRegistrar>().SingleInstance();

            builder.RegisterType<DataSourceService>().As<IDataSourceService>().InstancePerDependency();
            builder.RegisterType<EnumerationService>().As<IEnumerationService>().InstancePerDependency();
            builder.RegisterType<ReportService>().As<IReportService>().InstancePerDependency();
            builder.RegisterType<ReportTableService>().As<IReportTableService>().InstancePerDependency();
            builder.RegisterType<ReportTableColumnService>().As<IReportTableColumnService>().InstancePerDependency();
            builder.RegisterType<ReportGroupService>().As<IReportGroupService>().InstancePerDependency();
            builder.RegisterType<ReportGroupRoleService>().As<IReportGroupRoleService>().InstancePerDependency();
            builder.RegisterType<ReportSortingService>().As<IReportSortingService>().InstancePerDependency();
            builder.RegisterType<ReportUserBlacklistService>().As<IReportUserBlacklistService>().InstancePerDependency();

            // Transform Functions
            builder.RegisterType<BeautifyJsonFunction>().As<ITransformFunction>().SingleInstance();
            builder.RegisterType<ConvertTimeZoneFunction>().As<ITransformFunction>().SingleInstance();
            //builder.RegisterType<RoundUpFunction>().As<ITransformFunction>().SingleInstance();

            builder.RegisterType<ReportBuilderService>().As<IReportBuilderService>().InstancePerDependency();
        }
    }
}