using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Models;
using DynamicAdmin.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components.ModalDialogs;

public partial class EditEntityDialog<TEntity>
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public TEntity SelectedEntity { get; set; }
    [Parameter] public string TableName { get; set; }
    [Parameter] public EventCallback<TEntity> OnSave { get; set; }
    [Parameter] public EventCallback OnCloseModal { get; set; }

    [Inject] private IDataService<TEntity> DataService { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    private EntityViewModel<TEntity> _selectedEntity;
    private Dictionary<string, object> _inputValues = new();
    private Dictionary<string, string> _inputStringValues = new();

    protected override async Task OnParametersSetAsync()
    {
        _selectedEntity = await DataService.GetEntityViewModel(SelectedEntity);

        _inputValues.Clear();
        _inputStringValues.Clear();

        foreach (var prop in _selectedEntity.Properties)
        {
            if (!_inputValues.ContainsKey(prop.Name))
            {
                _inputValues[prop.Name] = prop.Value;
            }

            if (!_inputStringValues.ContainsKey(prop.Name))
            {
                _inputStringValues[prop.Name] = _inputValues[prop.Name]?.ToString();
            }
        }
    }

    private async Task UpdateEntity()
    {
        try
        {
            if (_selectedEntity == null)
            {
                await JSRuntime.InvokeVoidAsync("alert", "No item selected for editing.");
                return;
            }

            foreach (var prop in _selectedEntity.GetPropertiesWithoutRelations())
            {
                if (_inputStringValues.ContainsKey(prop.Name))
                {
                    ClassHelper.SetStringValue(_inputStringValues[prop.Name], prop.TablePropertyInfo,
                        _selectedEntity.Entity);
                }
            }

            await DataService.UpdateAsync(TableName, _selectedEntity.Entity);
            await OnSave.InvokeAsync(_selectedEntity.Entity);
            await IsOpenChanged.InvokeAsync(false);
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error while saving: {ex.Message}");
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task CloseModal()
    {
        await IsOpenChanged.InvokeAsync(false);
    }
}