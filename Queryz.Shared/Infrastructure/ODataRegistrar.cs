using System;
using Extenso.AspNetCore.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Queryz.Data.Domain;
using Queryz.Models;

namespace Queryz.Infrastructure
{
    public class ODataRegistrar : IODataRegistrar
    {
        public void Register(IRouteBuilder routes, IServiceProvider services)
        {
            var builder = GetBuilder(services);
            routes.MapODataServiceRoute("OData", "odata", builder.GetEdmModel());
        }

        public void Register(IEndpointRouteBuilder endpoints, IServiceProvider services)
        {
            var builder = GetBuilder(services);
            endpoints.MapODataRoute("OData", "odata", builder.GetEdmModel());
        }

        private ODataModelBuilder GetBuilder(IServiceProvider services)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(services);

            builder.EntitySet<DataSource>("DataSourceApi");
            builder.EntitySet<Enumeration>("EnumerationApi");
            builder.EntitySet<Report>("ReportApi");
            builder.EntitySet<ReportGroup>("ReportGroupApi");
            //builder.EntitySet<ReportGroupRole>("ReportGroupRoleApi");
            builder.EntitySet<ReportSorting>("ReportSortingApi");
            builder.EntitySet<ReportTable>("ReportTableApi");
            builder.EntitySet<ReportTableColumn>("ReportTableColumnApi");

            RegisterDataSourceODataActions(builder);
            RegisterReportGroupODataActions(builder);

            return builder;
        }

        private static void RegisterDataSourceODataActions(ODataModelBuilder builder)
        {
            var saveAction = builder.EntityType<DataSource>().Collection.Action("Save");
            saveAction.Parameter<int>("Id");
            saveAction.Parameter<string>("Name");
            saveAction.Parameter<DataProvider>("DataProvider");
            saveAction.Parameter<string>("ConnectionDetails");
            saveAction.ReturnsFromEntitySet<DataSource>("DataSourceApi");
        }

        private static void RegisterReportGroupODataActions(ODataModelBuilder builder)
        {
            var getReportsFunction = builder.EntityType<ReportGroup>().Collection.Function("GetReports");
            getReportsFunction.Parameter<int>("groupId");
            getReportsFunction.ReturnsFromEntitySet<Report>("ReportApi");

            var getEmailReportsFunction = builder.EntityType<ReportGroup>().Collection.Function("GetEmailReports");
            getEmailReportsFunction.Parameter<int>("groupId");
            getEmailReportsFunction.ReturnsFromEntitySet<Report>("ReportApi");

            var getRolesFunction = builder.EntityType<ReportGroup>().Collection.Function("GetRoles");
            getRolesFunction.Parameter<int>("id");
            getRolesFunction.ReturnsCollection<EdmRole>();

            var setRolesAction = builder.EntityType<ReportGroup>().Collection.Action("SetRoles");
            setRolesAction.Parameter<int>("id");
            setRolesAction.CollectionParameter<string>("roles");
            setRolesAction.Returns<IActionResult>();
        }
    }
}