using System.Reflection;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components.ModalDialogs;

public partial class CreateEntityDialog<TEntity> where TEntity : class
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public string EntityName { get; set; }
    [Parameter] public EventCallback<TEntity> OnSave { get; set; }
    [Parameter] public EventCallback OnCloseModal { get; set; }

    [Inject] private IDataService<TEntity> DataService { get; set; }
    [Inject] private IDataMapperService<TEntity> DataMapperService { get; set; }

    [Inject] private IDbInfoService DbInfoService { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    //TODO create strategies for different scenario of creating entity (withGeneric and with Object)
    private EntityViewModel<TEntity> _newEntity;
    private EntityViewModel<object> _newEntityObject;

    private IEnumerable<EntityProperty> _properties;

    private Dictionary<string, object> _inputValues = new();
    private Dictionary<string, string> _inputStringValues = new();

    private Dictionary<string, string> _entityNames = new();

    protected override async Task OnParametersSetAsync()
    {
        if (_newEntityObject != null)
        {
            _properties = _newEntityObject.Properties;
        }
        else
        {
            TEntity newEntity = Activator.CreateInstance<TEntity>();
            _newEntity = await DataMapperService.GetEntityViewModel(newEntity);
            _properties = _newEntity.Properties;
        }

        _inputValues.Clear();
        _inputStringValues.Clear();

        foreach (var prop in _properties)
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

    protected override Task OnInitializedAsync()
    {
        InitializeEntityNames();

        return Task.CompletedTask;
    }
    
    private void InitializeEntityNames()
    {
        _entityNames[EntityName] = EntityName;
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

            await DataService.CreateAsync(EntityName, newItem);
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
        _inputStringValues = await _properties.GenerateTestData();
        StateHasChanged();
    }

    private async Task CreateRelationEntity(EntityProperty propertyInfo)
    {
        _newEntityObject = await DataMapperService.GetEntityViewModel(propertyInfo.Value);
        EntityName = await DbInfoService.GetEntityName(propertyInfo.TablePropertyInfo);
        await OnParametersSetAsync();
    }

    private Task LoadTableNames()
    {
        _entityNames.Clear();
        foreach (var item in DbInfoService.GetEntityNames())
        {
            _entityNames[item] = item;
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnSelectedEntityName(string entityName)
    {
        EntityName = entityName;
        var entityType = await DbInfoService.GetEntityType(entityName);
        var entity = Activator.CreateInstance(entityType);
        _newEntityObject = await DataMapperService.GetEntityViewModel(entity);

        await OnParametersSetAsync();

        StateHasChanged();
    }
}