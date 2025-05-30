using Extenso;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Queryz.Data.Entities;
using Queryz.Extensions;

namespace Queryz.Controllers.Api;

public class DataSourceApiController : GenericODataController<DataSource, int>
{
    public DataSourceApiController(
        IAuthorizationService authorizationService,
        IRepository<DataSource> service)
        : base(authorizationService, service)
    {
    }

    [HttpPost]
    public virtual async Task<IActionResult> Save([FromBody] ODataActionParameters parameters)
    {
        int id = (int)parameters["Id"];
        string name = (string)parameters["Name"];
        var dataProvider = (DataProvider)parameters["DataProvider"];
        string connectionDetails = (string)parameters["ConnectionDetails"];

        string connectionString = dataProvider.GetConnectionString(connectionDetails, out var customProperties);

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