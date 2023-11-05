using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Models;
using DynamicAdmin.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components.ModalDialogs;

public partial class CreateEntityDialog<TEntity> {
    
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public string TableName { get; set; }
    [Parameter] public EventCallback<TEntity> OnSave { get; set; }
    [Parameter] public EventCallback OnCloseModal { get; set; }
    
    [Inject] private IDataService<TEntity> DataService { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    private EntityViewModel<TEntity> _newEntity;
    private Dictionary<string, object> _inputValues = new();
    private Dictionary<string, string> _inputStringValues = new();

    protected override async Task OnParametersSetAsync()
    {
        TEntity newEntity = Activator.CreateInstance<TEntity>();
        _newEntity = await DataService.GetEntityViewModel(newEntity);

        _inputValues.Clear();
        _inputStringValues.Clear();

        foreach (var prop in _newEntity.Properties)
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

    private async Task CreateEntity()
    {
        try
        {
            TEntity newItem = Activator.CreateInstance<TEntity>();
            foreach (var prop in _newEntity.GetPropertiesWithoutRelations())
            {
                if (_inputValues.ContainsKey(prop.Name))
                {
                    ClassHelper.SetStringValue(_inputStringValues[prop.Name], prop.TablePropertyInfo, newItem);
                }
            }

            await DataService.CreateAsync(TableName, newItem);
            await OnSave.InvokeAsync(newItem);
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

    private async Task GenerateTestDataForAllFields()
    {
        foreach (var tableProperty in _newEntity.Properties)
        {
            Type propType = tableProperty.TablePropertyInfo.PropertyType;

            if (propType == typeof(string))
            {
                _inputStringValues[tableProperty.Name] = $"{tableProperty.Name} TestString";
            }
            else if (ClassHelper.IsNumericType(propType))
            {
                _inputStringValues[tableProperty.Name] = "123";
            }
            else if (propType == typeof(DateTime) || propType == typeof(DateTime?))
            {
                _inputStringValues[tableProperty.Name] = DateTime.Now.ToString();
            }
            else
            {
                _inputStringValues[tableProperty.Name] = Guid.NewGuid().ToString();
            }
        }

        StateHasChanged(); // Request a UI update
    }

}
