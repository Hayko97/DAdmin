using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Components.EntityDialog.Strategies;

public interface IEntityDialogStrategy
{
    string EntityName { get; set; }
    Task CreateEntity();
    Task<IEnumerable<EntityProperty>> GetProperties();

    Task MapStringValuesToEntity();
}