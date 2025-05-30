using Queryz.Data.Entities;

namespace Queryz.Services;

public interface IEnumerationService : IGenericDataService<Enumeration>
{
}

public class EnumerationService : GenericDataService<Enumeration>, IEnumerationService
{
    public EnumerationService(IRepository<Enumeration> repository)
        : base(repository)
    {
    }
}