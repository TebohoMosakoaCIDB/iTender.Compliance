using iTender.Compliance.Application.DTOs;
using Microsoft.Xrm.Sdk;
using static iTender.Compliance.Domain.Constants.CrmFieldNames;


namespace iTender.Compliance.Infrastructure.Mappers
{
    public static class ClassOfWorkTypeMapper
    {
        public static ClassOfWorkTypeModel? ToDomain(Entity entity)
        {
            if (entity is null) return null;

            return new ClassOfWorkTypeModel
            {
                Id = entity.Id,
                Name = entity.GetAttributeValue<string>(ClassOfConstructionWorkFields.Name) ?? string.Empty,
                Description = entity.GetAttributeValue<string>(ClassOfConstructionWorkFields.Description)
            };
        }

        public static Entity ToEntity(ClassOfWorkTypeModel model)
        {
            var entity = model.Id != Guid.Empty
                ? new Entity(Domain.Constants.CrmEntityNames.ClassOfWorkType, model.Id)
                : new Entity(Domain.Constants.CrmEntityNames.ClassOfWorkType);

            entity[ClassOfConstructionWorkFields.Name] = model.Name;

            if (!string.IsNullOrWhiteSpace(model.Description))
                entity[ClassOfConstructionWorkFields.Description] = model.Description;

            return entity;
        }
    }

}
