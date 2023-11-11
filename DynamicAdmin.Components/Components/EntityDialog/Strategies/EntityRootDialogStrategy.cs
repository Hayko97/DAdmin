using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Components.EntityDialog.Strategies;

public class EntityRootDialogStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly CreateEntityDialog<TEntity> _dialog;

    public EntityRootDialogStrategy(
        CreateEntityDialog<TEntity> dialog
    )
    {
        _dialog = dialog;
    }

    public string EntityName { get; set; }

    public async Task CreateEntity()
    {
        TEntity newItem = Activator.CreateInstance<TEntity>();
        foreach (var prop in _dialog.RootEntity.GetPropertiesWithoutRelations())
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

    public async Task<IEnumerable<EntityProperty>> GetProperties()
    {
        if (_dialog.RootEntity == null ||
            !_dialog.RootEntity.Properties.Any(x =>
                x.IsDefaultValue
            )
           ) // In the case when no any change related rootEntity
        {
            TEntity newEntity = Activator.CreateInstance<TEntity>();
            _dialog.RootEntity = await _dialog.DataMapperService.GetEntityViewModel(newEntity);
        }

        return _dialog.RootEntity.Properties;
    }

    public Task MapStringValuesToEntity()
    {
        if (_dialog.RootEntity != null)
        {
            foreach (var prop in _dialog.RootEntity.GetPropertiesWithoutRelations())
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