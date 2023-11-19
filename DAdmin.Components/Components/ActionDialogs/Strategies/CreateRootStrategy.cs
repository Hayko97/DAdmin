using DAdmin.Components.Helpers;
using DAdmin.Shared.DTO;

namespace DAdmin.ActionDialogs.Strategies;

public class CreateRootStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly ResourceActionDialog<TEntity> _dialog;

    public CreateRootStrategy(
        ResourceActionDialog<TEntity> dialog
    )
    {
        _dialog = dialog;
    }

    public string EntityName { get; set; }

    public async Task Save()
    {
        TEntity newItem = Activator.CreateInstance<TEntity>();
        foreach (var prop in _dialog.RootEntityResource.GetPropertiesWithoutRelations())
        {
            if (_dialog.InputValues.ContainsKey(prop.Name))
            {
                ClassHelper.SetStringValue(_dialog.InputStringValues[prop.Name], prop.EntityPropertyInfo, newItem);
            }
        }

        await _dialog.DataService.CreateAsync(_dialog.EntityName, newItem);
        await _dialog.OnSave.InvokeAsync(newItem);
        await _dialog.CloseModal();
    }

    public async Task<IEnumerable<DataProperty>> GetProperties()
    {
        if (_dialog.RootEntityResource == null ||
            !_dialog.RootEntityResource.Properties.Any(x =>
                x.IsDefaultValue
            )
           ) // In the case when no any change related rootEntity
        {
            TEntity newEntity = Activator.CreateInstance<TEntity>();
            _dialog.RootEntityResource = await _dialog.DataMapperService.MapToTableResource(newEntity);
        }

        return _dialog.RootEntityResource.Properties;
    }

    public Task MapStringValuesToEntity()
    {
        if (_dialog.RootEntityResource != null)
        {
            foreach (var prop in _dialog.RootEntityResource.GetPropertiesWithoutRelations())
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