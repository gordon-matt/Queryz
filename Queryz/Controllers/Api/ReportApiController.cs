using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Queryz.Data.Entities;
using Queryz.Services;

namespace Queryz.Controllers.Api;

public class ReportApiController : GenericODataController<Report, int>
{
    private readonly IReportGroupRoleService reportGroupRoleService;
    private readonly IReportSortingService reportSortingService;
    private readonly IReportTableColumnService reportTableColumnService;
    private readonly IReportTableService reportTableService;
    private readonly IReportUserBlacklistService reportUserBlacklistService;

    public ReportApiController(
        IAuthorizationService authorizationService,
        IRepository<Report> repository,
        IReportGroupRoleService reportGroupRoleService,
        IReportSortingService reportSortingService,
        IReportTableColumnService reportTableColumnService,
        IReportTableService reportTableService,
        IReportUserBlacklistService reportUserBlacklistService)
        : base(authorizationService, repository)
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

    protected override async Task<IQueryable<Report>> ApplyMandatoryFilterAsync(IQueryable<Report> query)
    {
        query = await base.ApplyMandatoryFilterAsync(query);

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