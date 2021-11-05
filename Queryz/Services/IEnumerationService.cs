using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
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
}