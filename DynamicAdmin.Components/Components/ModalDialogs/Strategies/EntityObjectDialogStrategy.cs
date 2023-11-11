using DynamicAdmin.Components.Extensions;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Components.ModalDialogs.Strategies;

public class EntityObjectDialogStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly CreateEntityDialog<TEntity> _dialog;

    public EntityObjectDialogStrategy(CreateEntityDialog<TEntity> dialog)
    {
        _dialog = dialog;
    }

    public string EntityName { get; set; }

    public async Task CreateEntity()
    {
        var newObjectItem = _dialog.ObjectEntity.EntityModel; // Current Object

        // Set Input Values to Properties
        foreach (var prop in _dialog.ObjectEntity.GetPropertiesWithoutRelations())
        {
            if (_dialog.InputValues.ContainsKey(prop.Name))
            {
                ClassHelper.SetStringValue(_dialog.InputStringValues[prop.Name], prop.TablePropertyInfo, newObjectItem);
            }
        }

        await _dialog.DataService.CreateAsync(_dialog.EntityName, newObjectItem);

        if (_dialog.EntityObjectStack.Any())
        {
            _dialog.ObjectEntity = _dialog.EntityObjectStack.Pop(); // Set Previous object
            _dialog.ObjectEntity = await _dialog.DataMapperService.GetEntityViewModel(_dialog.ObjectEntity.EntityModel);
            EntityName = _dialog.ObjectEntity.Name;
        }
        else
        {
            _dialog.ObjectEntity = null; // Null indicates initializing EntityRootDialogStrategy after
            _dialog.RootEntity = await _dialog.DataMapperService.GetEntityViewModel(_dialog.RootEntity.EntityModel);
            EntityName = _dialog.RootEntity.Name;
        }
    }

    public Task<IEnumerable<EntityProperty>> GetProperties()
    {
        return Task.FromResult(_dialog.ObjectEntity.Properties);
    }

    public Task MapStringValuesToEntity()
    {
        if (_dialog.ObjectEntity != null)
        {
            foreach (var prop in _dialog.ObjectEntity.GetPropertiesWithoutRelations())
            {
                if (_dialog.InputValues.ContainsKey(prop.Name))
                {
                    prop.Value = _dialog.InputStringValues[prop.Name];
                    prop.IsDefaultValue = string.IsNullOrEmpty(_dialog.InputStringValues[prop.Name]);
                }
            }
        }

        return Task.CompletedTask;
    }
}