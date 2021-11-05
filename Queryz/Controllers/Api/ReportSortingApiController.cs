using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Controllers.Api
{
    public class ReportSortingApiController : GenericODataController<ReportSorting, int>
    {
        public ReportSortingApiController(IRepository<ReportSorting> repository)
            : base(repository)
        {
        }

        protected override int GetId(ReportSorting entity) => entity.Id;

        protected override void SetNewId(ReportSorting entity)
        {
        }
    }
}