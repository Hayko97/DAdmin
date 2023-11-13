using DAdmin.Components.Helpers;
using DAdmin.Shared.DTO;
using Microsoft.JSInterop;

namespace DAdmin.Components.Components.EntityDialog.Strategies;

public class EditRootStrategy<TEntity> : IEntityDialogStrategy where TEntity : class
{
    private readonly EntityActionDialog<TEntity> _dialog;
    private readonly TEntity? _entity;

    public EditRootStrategy(
        EntityActionDialog<TEntity> dialog,
        TEntity? entity
    )
    {
        _dialog = dialog;
        _entity = entity;
    }

    public string EntityName { get; set; }

    public async Task Save()
    {
        if (_entity == null)
        {
            await _dialog.JSRuntime.InvokeVoidAsync("alert", "No item selected for editing.");
            return;
        }

        foreach (var prop in _dialog.RootTableResource.GetPropertiesWithoutRelations())
        {
            if (_dialog.InputStringValues.ContainsKey(prop.Name))
            {
                ClassHelper.SetStringValue(_dialog.InputStringValues[prop.Name], prop.TablePropertyInfo,
                    _dialog.RootTableResource.EntityModel
                );
            }
        }

        await _dialog.DataService.UpdateAsync(EntityName, _dialog.RootTableResource.EntityModel);
        await _dialog.OnSave.InvokeAsync(_dialog.RootTableResource.EntityModel);
        await _dialog.CloseModal();
    }

    public async Task<IEnumerable<ResourceProperty>> GetProperties()
    {
        if (_entity == null)
        {
            throw new ArgumentException("No Selected Resources");
        }
        
        if (_dialog.RootTableResource == null ||
            !_dialog.RootTableResource.Properties.Any(x =>
                x.IsDefaultValue
            )
           ) // In the case when no any change related rootEntity
        {
            _dialog.RootTableResource = await _dialog.DataMapperService.MapToTableResource(_entity);
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