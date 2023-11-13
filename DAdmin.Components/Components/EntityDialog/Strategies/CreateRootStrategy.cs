using DAdmin.Components.Helpers;
using DAdmin.Shared.DTO;

namespace DAdmin.Components.Components.EntityDialog.Strategies;

public class CreateRootStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly EntityActionDialog<TEntity> _dialog;

    public CreateRootStrategy(
        EntityActionDialog<TEntity> dialog
    )
    {
        _dialog = dialog;
    }

    public string EntityName { get; set; }

    public async Task Save()
    {
        TEntity newItem = Activator.CreateInstance<TEntity>();
        foreach (var prop in _dialog.RootTableResource.GetPropertiesWithoutRelations())
        {
            if (_dialog.InputValues.ContainsKey(prop.Name))
            {
                ClassHelper.SetStringValue(_dialog.InputStringValues[prop.Name], prop.TablePropertyInfo, newItem);
            }
        }

        await _dialog.DataService.CreateAsync(_dialog.EntityName, newItem);
        await _dialog.OnSave.InvokeAsync(newItem);
        await _dialog.CloseModal();
    }

    public async Task<IEnumerable<ResourceProperty>> GetProperties()
    {
        if (_dialog.RootTableResource == null ||
            !_dialog.RootTableResource.Properties.Any(x =>
                x.IsDefaultValue
            )
           ) // In the case when no any change related rootEntity
        {
            TEntity newEntity = Activator.CreateInstance<TEntity>();
            _dialog.RootTableResource = await _dialog.DataMapperService.MapToTableResource(newEntity);
        }

        return _dialog.RootTableResource.Properties;
    }

    public Task MapStringValuesToEntity()
    {
        if (_dialog.RootTableResource != null)
        {
            foreach (var prop in _dialog.RootTableResource.GetPropertiesWithoutRelations())
            {
                if (_dialog.InputValues.ContainsKey(prop.Name))
                {
                    prop.Value = _dialog.InputStringValues[prop.Name];
                    prop.IsDefaultValue = false;
                }
            }
        }

        return Task.CompletedTask;
    }
}