﻿using System.Linq;
using System.Threading.Tasks;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Queryz.Data.Domain;
using Queryz.Services;

namespace Queryz.Controllers.Api
{
    public class ReportApiController : GenericODataController<Report, int>
    {
        private readonly IReportGroupRoleService reportGroupRoleService;
        private readonly IReportSortingService reportSortingService;
        private readonly IReportTableColumnService reportTableColumnService;
        private readonly IReportTableService reportTableService;
        private readonly IReportUserBlacklistService reportUserBlacklistService;

        public ReportApiController(
            IRepository<Report> repository,
            IReportGroupRoleService reportGroupRoleService,
            IReportSortingService reportSortingService,
            IReportTableColumnService reportTableColumnService,
            IReportTableService reportTableService,
            IReportUserBlacklistService reportUserBlacklistService)
            : base(repository)
        {
            this.reportGroupRoleService = reportGroupRoleService;
            this.reportSortingService = reportSortingService;
            this.reportTableColumnService = reportTableColumnService;
            this.reportTableService = reportTableService;
            this.reportUserBlacklistService = reportUserBlacklistService;
        }

        protected override int GetId(Report entity) => entity.Id;

        protected override void SetNewId(Report entity)
        {
        }

        protected override IQueryable<Report> ApplyMandatoryFilter(IQueryable<Report> query)
        {
            query = base.ApplyMandatoryFilter(query);

            if (!User.IsInRole("Administrators"))
            {
                query = query.Where(x => x.Enabled);
            }

            return query;
        }

        public override async Task<IActionResult> Delete([FromODataUri] int key)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            var entity = await Repository.FindOneAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            await reportUserBlacklistService.DeleteAsync(x => x.ReportId == key);
            await reportGroupRoleService.DeleteAsync(x => x.ReportGroupId == key);
            await reportSortingService.DeleteAsync(x => x.ReportId == key);
            await reportTableColumnService.DeleteAsync(x => x.ReportId == key);
            await reportTableService.DeleteAsync(x => x.ReportId == key);

            await Repository.DeleteAsync(entity);

            return NoContent();
        }
    }
}