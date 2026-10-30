using Microsoft.Extensions.DependencyInjection;
using Queryz.Services.Reports;

namespace Queryz.Services;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddQueryzServices()
        {
            services.AddScoped<IDataSourceService, DataSourceService>();
            services.AddScoped<IEnumerationService, EnumerationService>();
            services.AddScoped<IReportGroupService, ReportGroupService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ISchemaService, SchemaService>();
            return services;
        }
    }
}
