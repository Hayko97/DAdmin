using DAdmin.Shared.DTO;

namespace DAdmin.Components.Components.EntityDialog.Strategies;

public interface IEntityDialogStrategy
{
    string EntityName { get; set; }
    Task Save();
    Task<IEnumerable<ResourceProperty>> GetProperties();

    Task MapStringValuesToEntity();
}