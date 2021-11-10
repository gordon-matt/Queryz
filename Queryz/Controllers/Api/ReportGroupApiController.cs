using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Queryz.Data;
using Queryz.Data.Domain;
using Queryz.Models;
using Queryz.Services;

namespace Queryz.Controllers.Api
{
    public class ReportGroupApiController : GenericODataController<ReportGroup, int>
    {
        private readonly IDbContextFactory dbContextFactory;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IReportService reportService;
        private readonly IReportGroupRoleService reportGroupRoleService;
        private readonly IReportSortingService reportSortingService;
        private readonly IReportTableColumnService reportTableColumnService;
        private readonly IReportTableService reportTableService;
        private readonly IReportUserBlacklistService reportUserBlacklistService;

        public ReportGroupApiController(
            IDbContextFactory dbContextFactory,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IRepository<ReportGroup> repository,
            IReportService reportService,
            IReportGroupRoleService reportGroupRoleService,
            IReportSortingService reportSortingService,
            IReportTableColumnService reportTableColumnService,
            IReportTableService reportTableService,
            IReportUserBlacklistService reportUserBlacklistService)
            : base(repository)
        {
            this.dbContextFactory = dbContextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.reportService = reportService;
            this.reportGroupRoleService = reportGroupRoleService;
            this.reportSortingService = reportSortingService;
            this.reportTableColumnService = reportTableColumnService;
            this.reportTableService = reportTableService;
            this.reportUserBlacklistService = reportUserBlacklistService;
        }

        public override async Task<IActionResult> Get(ODataQueryOptions<ReportGroup> options)
        {
            // No need for this check, because we filter by group roles below.
            //if (!CheckPermission(ReadPermission))
            //{
            //    return Unauthorized();
            //}

            var connection = GetDisposableConnection();
            var query = connection.Query();
            query = ApplyMandatoryFilter(query);

            if (!User.IsInRole(Constants.Roles.Administrators))
            {
                query = query.Include(x => x.ReportGroupRoles);
                var user = await userManager.FindByNameAsync(User.Identity.Name);
                var roles = await userManager.Users.Include(x => x.Roles).Where(x => x.Id == user.Id).ToListAsync();
                var roleIds = roles.Select(x => x.Id).ToArray();
                query = query.Where(x => x.ReportGroupRoles.Any(y => roleIds.Contains(y.RoleId)));
            }

            var results = options.ApplyTo(query, IgnoreQueryOptions);

            var response = await Task.FromResult((results as IQueryable<ReportGroup>).ToHashSet());
            return Ok(response);
        }

        protected override int GetId(ReportGroup entity) => entity.Id;

        protected override void SetNewId(ReportGroup entity)
        {
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

            var reportIds = Enumerable.Empty<int>();
            using (var connection = reportService.OpenConnection())
            {
                reportIds = await connection.Query(x => x.GroupId == key).Select(x => x.Id).ToArrayAsync();
            }

            await reportGroupRoleService.DeleteAsync(x => reportIds.Contains(x.ReportGroupId));
            await reportSortingService.DeleteAsync(x => reportIds.Contains(x.ReportId));
            await reportTableColumnService.DeleteAsync(x => reportIds.Contains(x.ReportId));
            await reportTableService.DeleteAsync(x => reportIds.Contains(x.ReportId));
            await reportService.DeleteAsync(x => reportIds.Contains(x.Id));

            await Repository.DeleteAsync(entity);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetReports([FromODataUri] int groupId, ODataQueryOptions<Report> options)
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Unauthorized();
            }

            options.Validate(new ODataValidationSettings
            {
                AllowedQueryOptions = AllowedQueryOptions.All
            });

            var query = (await reportService
                .FindAsync(
                    x => x.GroupId == groupId,
                    include => include.Group,
                    include => include.Group.ReportGroupRoles))
                .AsQueryable();

            if (!User.IsInRole("Administrators"))
            {
                var user = await userManager.FindByNameAsync(User.Identity.Name);
                var deniedReportIds = (await GetUserDeniedReportIdsAsync(user.Id)).ToArray();
                query = query.Where(x => x.Enabled && !deniedReportIds.Contains(x.Id));
            }

            var results = options.ApplyTo(query);
            return Ok((results as IQueryable<Report>).ToHashSet());
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles([FromODataUri] int id, ODataQueryOptions<Report> options) //TODO: See if can remove options here..
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Unauthorized();
            }

            var reportGroupRoles = await reportGroupRoleService.FindAsync(x => x.ReportGroupId == id);
            var roleIds = reportGroupRoles.Select(x => x.RoleId).ToList();
            var roles = await roleManager.Roles.Where(x => roleIds.Contains(x.Id)).ToListAsync();
            var results = roles.Select(x => new EdmRole
            {
                Id = x.Id,
                Name = x.Name
            });
            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> SetRoles(ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            int reportGroupId = (int)parameters["id"];
            var roleIds = (IEnumerable<string>)parameters["roles"];

            var reportGroup = await Repository.FindOneAsync(reportGroupId);

            using (var context = (ApplicationDbContext)dbContextFactory.GetContext())
            {
                var currentRoleIds = from rgr in context.ReportGroupRoles
                                     join r in context.Roles on rgr.RoleId equals r.Id
                                     where rgr.ReportGroupId == reportGroupId
                                     select rgr.RoleId;

                var toDelete = from rgr in context.ReportGroupRoles
                               join r in context.Roles on rgr.RoleId equals r.Id
                               where rgr.ReportGroupId == reportGroupId
                               && !roleIds.Contains(rgr.RoleId)
                               select rgr;

                var toAdd = roleIds.Where(x => !currentRoleIds.Contains(x)).Select(x => new ReportGroupRole
                {
                    ReportGroupId = reportGroupId,
                    RoleId = x
                });

                if (toDelete.Any())
                {
                    context.ReportGroupRoles.RemoveRange(toDelete);
                }

                if (toAdd.Any())
                {
                    context.ReportGroupRoles.AddRange(toAdd);
                }

                await context.SaveChangesAsync();
            }

            return Ok();
        }

        private async Task<IEnumerable<int>> GetUserDeniedReportIdsAsync(string userId)
        {
            var query = await reportUserBlacklistService.FindAsync(x => x.UserId == userId);
            return query.Select(x => x.ReportId).ToList();
        }
    }
}