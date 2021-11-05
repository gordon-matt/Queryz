using System.Collections.Generic;
using System.Threading.Tasks;
using Extenso;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Queryz.Data.Domain;
using Queryz.Extensions;

namespace Queryz.Controllers.Api
{
    public class DataSourceApiController : GenericODataController<DataSource, int>
    {
        public DataSourceApiController(IRepository<DataSource> service)
            : base(service)
        {
        }

        [HttpPost]
        public virtual async Task<IActionResult> Save([FromBody] ODataActionParameters parameters)
        {
            int id = (int)parameters["Id"];
            string name = (string)parameters["Name"];
            var dataProvider = (DataProvider)parameters["DataProvider"];
            string connectionDetails = (string)parameters["ConnectionDetails"];

            IDictionary<string, string> customProperties;
            string connectionString = dataProvider.GetConnectionString(connectionDetails, out customProperties);

            var entity = new DataSource
            {
                Id = id,
                Name = name,
                DataProvider = dataProvider,
                ConnectionString = connectionString,
                CustomProperties = customProperties.JsonSerialize()
            };

            if (id > 0)
            {
                await Repository.UpdateAsync(entity);
                return Updated(entity);
            }
            else
            {
                await Repository.InsertAsync(entity);
                return Created(entity);
            }
        }

        protected override int GetId(DataSource entity) => entity.Id;

        protected override void SetNewId(DataSource entity)
        {
        }
    }
}