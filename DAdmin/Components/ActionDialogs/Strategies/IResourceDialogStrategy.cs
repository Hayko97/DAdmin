using DAdmin.Dto;

namespace DAdmin.ActionDialogs.Strategies;

public interface IResourceDialogStrategy
{
    string EntityName { get; set; }
    Task Save();
    Task<IEnumerable<DataProperty>> GetProperties();

    Task MapStringValuesToEntity();
}