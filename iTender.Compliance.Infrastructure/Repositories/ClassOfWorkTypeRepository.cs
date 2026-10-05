using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Constants;
using iTender.Compliance.Infrastructure.CRM;
using iTender.Compliance.Infrastructure.Mappers;
using Microsoft.Xrm.Sdk.Query;
using static iTender.Compliance.Domain.Constants.CrmFieldNames;

namespace iTender.Compliance.Infrastructure.Repositories
{
    public class ClassOfWorkTypeRepository : IClassOfWorkTypeRepository
    {
        private readonly ICrmServiceFactory _crmServiceFactory;

        public ClassOfWorkTypeRepository(ICrmServiceFactory crmServiceFactory)
        {
            _crmServiceFactory = crmServiceFactory;
        }

        public Task<IReadOnlyList<ClassOfWorkTypeModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var query = new QueryExpression(CrmEntityNames.ClassOfWorkType)
            {
                ColumnSet = new ColumnSet(
                    ClassOfConstructionWorkFields.Id,
                    ClassOfConstructionWorkFields.Name,
                    ClassOfConstructionWorkFields.Description)
            };

            query.AddOrder(ClassOfConstructionWorkFields.Name, OrderType.Ascending);

            var service = _crmServiceFactory.Create();
            var result = service.RetrieveMultiple(query);

            IReadOnlyList<ClassOfWorkTypeModel> models = result.Entities
                .Select(ClassOfWorkTypeMapper.ToDomain)
                .Where(m => m is not null)
                .Select(m => m!)
                .ToList();

            return Task.FromResult(models);
        }

        public async Task<ClassOfWorkTypeModel?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var all = await GetAllAsync(cancellationToken);

            return all.FirstOrDefault(c =>
                string.Equals(c.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
