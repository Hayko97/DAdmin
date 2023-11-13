using DAdmin.Components.Extensions;
using DAdmin.Components.Helpers;
using DAdmin.Shared.DTO;

namespace DAdmin.Components.Components.EntityDialog.Strategies;

public class CreateObjectStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly EntityActionDialog<TEntity> _dialog;

    public CreateObjectStrategy(EntityActionDialog<TEntity> dialog)
    {
        _dialog = dialog;
    }

    public string EntityName { get; set; }

    public async Task Save()
    {
        var newObjectItem = _dialog.ObjectTableResource.EntityModel; // Current Object

        // Set Input Values to Properties
        foreach (var prop in _dialog.ObjectTableResource.GetPropertiesWithoutRelations())
        {
            if (_dialog.InputValues.ContainsKey(prop.Name))
            {
                ClassHelper.SetStringValue(_dialog.InputStringValues[prop.Name], prop.TablePropertyInfo, newObjectItem);
            }
        }

        await _dialog.DataService.CreateAsync(_dialog.EntityName, newObjectItem);

        if (_dialog.TableResourcesStack.Any())
        {
            _dialog.ObjectTableResource = _dialog.TableResourcesStack.Pop(); // Set Previous object
            _dialog.ObjectTableResource = await _dialog.DataMapperService.MapToTableResource(_dialog.ObjectTableResource.EntityModel);
            EntityName = _dialog.ObjectTableResource.Name;
        }
        else
        {
            _dialog.ObjectTableResource = null; // Null indicates initializing CreateRootStrategy after
            _dialog.RootTableResource = await _dialog.DataMapperService.MapToTableResource(_dialog.RootTableResource.EntityModel);
            EntityName = _dialog.RootTableResource.Name;
        }
    }

    public Task<IEnumerable<ResourceProperty>> GetProperties()
    {
        return Task.FromResult(_dialog.ObjectTableResource.Properties);
    }

    public Task MapStringValuesToEntity()
    {
        if (_dialog.ObjectTableResource != null)
        {
            foreach (var prop in _dialog.ObjectTableResource.GetPropertiesWithoutRelations())
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