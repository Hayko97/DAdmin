using DAdmin.Shared.DTO;

namespace DAdmin.ActionDialogs.Strategies;

public interface IEntityDialogStrategy
{
    string EntityName { get; set; }
    Task Save();
    Task<IEnumerable<DataProperty>> GetProperties();

    Task MapStringValuesToEntity();
}