using DAdmin.Components.Extensions;
using DAdmin.Components.Helpers;
using DAdmin.Shared.DTO;

namespace DAdmin.Components.Components.EntityDialog.Strategies;

public class CreateObjectStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly ResourceActionDialog<TEntity> _dialog;

    public CreateObjectStrategy(ResourceActionDialog<TEntity> dialog)
    {
        _dialog = dialog;
    }

    public string EntityName { get; set; }

    public async Task Save()
    {
        var newObjectItem = _dialog.CurrentChildResource.EntityModel; // Current Object

        // Set Input Values to Properties
        foreach (var prop in _dialog.CurrentChildResource.GetPropertiesWithoutRelations())
        {
            if (_dialog.InputValues.ContainsKey(prop.Name))
            {
                ClassHelper.SetStringValue(_dialog.InputStringValues[prop.Name], prop.EntityPropertyInfo, newObjectItem);
            }
        }

        await _dialog.DataService.CreateAsync(_dialog.EntityName, newObjectItem);

        if (_dialog.ChildDataResourcesStack.Any())
        {
            _dialog.CurrentChildResource = _dialog.ChildDataResourcesStack.Pop(); // Set Previous object
            _dialog.CurrentChildResource = await _dialog.DataMapperService.MapToTableResource(_dialog.CurrentChildResource.EntityModel);
            EntityName = _dialog.CurrentChildResource.Name;
        }
        else
        {
            _dialog.CurrentChildResource = null; // Null indicates initializing CreateRootStrategy after
            _dialog.RootEntityResource = await _dialog.DataMapperService.MapToTableResource(_dialog.RootEntityResource.EntityModel);
            EntityName = _dialog.RootEntityResource.Name;
        }
    }

    public Task<IEnumerable<ResourceProperty>> GetProperties()
    {
        return Task.FromResult(_dialog.CurrentChildResource.Properties);
    }

    public Task MapStringValuesToEntity()
    {
        if (_dialog.CurrentChildResource != null)
        {
            foreach (var prop in _dialog.CurrentChildResource.GetPropertiesWithoutRelations())
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