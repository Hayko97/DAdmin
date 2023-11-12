using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components;

public partial class EditEntityDialog<TEntity> where TEntity : class
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public TEntity SelectedEntity { get; set; }
    [Parameter] public string EntityName { get; set; }
    [Parameter] public EventCallback<TEntity> OnSave { get; set; }
    [Parameter] public EventCallback OnCloseModal { get; set; }

    [Inject] private IDataService<TEntity> DataService { get; set; }
    [Inject] private IDataMapperService<TEntity> DataMapperService { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    private Dictionary<string, object> _inputValues = new();
    private Dictionary<string, string> _inputStringValues = new();
    
    private Entity<TEntity> _entity = new();

    protected override async Task OnParametersSetAsync()
    {
        _inputValues.Clear();
        _inputStringValues.Clear();

        if (SelectedEntity != null)
        {
            _entity = await DataMapperService.GetEntityViewModel(SelectedEntity);
            foreach (var prop in _entity.Properties)
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
    }

    private async Task UpdateEntity()
    {
        try
        {
            if (SelectedEntity == null)
            {
                await JSRuntime.InvokeVoidAsync("alert", "No item selected for editing.");
                return;
            }

            foreach (var prop in _entity.GetPropertiesWithoutRelations())
            {
                if (_inputStringValues.ContainsKey(prop.Name))
                {
                    ClassHelper.SetStringValue(_inputStringValues[prop.Name], prop.TablePropertyInfo,
                        _entity.EntityModel);
                }
            }

            await DataService.UpdateAsync(EntityName, _entity.EntityModel);
            await OnSave.InvokeAsync(_entity.EntityModel);
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