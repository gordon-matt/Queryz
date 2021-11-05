using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IDataSourceService : IGenericDataService<DataSource>
    {
    }

    public class DataSourceService : GenericDataService<DataSource>, IDataSourceService
    {
        public DataSourceService(IRepository<DataSource> repository)
            : base(repository)
        {
        }
    }
}