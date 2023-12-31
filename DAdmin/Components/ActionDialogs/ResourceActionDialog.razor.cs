using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAdmin.ActionDialogs;
using DAdmin.ActionDialogs.Strategies;
using DAdmin.Components.ActionDialogs.Enums;
using DAdmin.Dto;
using DAdmin.Extensions;
using Microsoft.AspNetCore.Components;
using DAdmin.Helpers;
using DAdmin.Services.DbServices.Interfaces;
using Microsoft.JSInterop;

namespace DAdmin;

public partial class ResourceActionDialog<TEntity> : DAdminComponent where TEntity : class
{
    #region Parameters

    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public string EntityName { get; set; }
    [Parameter] public DialogMode DialogMode { get; set; }
    [Parameter] public TEntity? SelectedEntity { get; set; }
    [Parameter] public EventCallback<TEntity> OnSave { get; set; }
    [Parameter] public EventCallback OnCloseModal { get; set; }

    #endregion

    #region Dependency Injection

    [Inject] public IDataService<TEntity> DataService { get; set; }
    [Inject] public IDataMapperService<TEntity> DataMapperService { get; set; }
    [Inject] public IDbInfoService DbInfoService { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }

    #endregion

    #region Private Fields

    private IEnumerable<DataProperty> _properties;
    private IResourceDialogStrategy _resourceDialogStrategy;
    private Dictionary<string, string> _entityNames = new();

    #endregion

    #region Public Properties

    public DataResourceDto<TEntity>? RootEntityResource { get; set; }
    public DataResourceDto<object>? CurrentChildResource { get; set; }
    public Dictionary<string, object> InputValues { get; set; } = new();
    public Dictionary<string, string> InputStringValues { get; set; } = new();
    public Stack<DataResourceDto<object>> ChildDataResourcesStack { get; set; } = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await LoadTableNames();
    }

    protected override async Task OnParametersSetAsync()
    {
        await InitializeStrategy();
    }

    #endregion

    #region Strategy Initialization and Handling

    public async Task InitializeStrategy()
    {
        if (CurrentChildResource?.Properties == null && DialogMode == DialogMode.Create)
        {
            _resourceDialogStrategy = new CreateRootStrategy<TEntity>(this);
        }
        else if (CurrentChildResource?.Properties != null && DialogMode == DialogMode.Create)
        {
            _resourceDialogStrategy = new CreateObjectStrategy<TEntity>(this);
        }
        else if (CurrentChildResource?.Properties == null && DialogMode == DialogMode.Edit)
        {
            _resourceDialogStrategy = new EditRootStrategy<TEntity>(this, SelectedEntity);
        }
        else if (CurrentChildResource?.Properties != null && DialogMode == DialogMode.Edit)
        {
            _resourceDialogStrategy = new EditRootStrategy<TEntity>(this, SelectedEntity);
        }
        else
        {
            throw new ArgumentException("Invalid strategy");
        }

        _properties = await _resourceDialogStrategy.GetProperties();
        PrepareInputValues();
        await _resourceDialogStrategy.MapStringValuesToEntity();
    }

    private void PrepareInputValues()
    {
        InputValues.Clear();
        InputStringValues.Clear();

        foreach (var prop in _properties)
        {
            InputValues.TryAdd(prop.Name, prop.Value);
            InputStringValues.TryAdd(prop.Name, prop.IsDefaultValue ? string.Empty : prop.Value?.ToString());
        }
    }

    #endregion

    #region Action Handlers

    private async Task Save()
    {
        try
        {
            await Task.Delay(400);
            await _resourceDialogStrategy.Save();
            EntityName = _resourceDialogStrategy.EntityName;
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error while saving: {ex.Message}");
        }

        await InitializeStrategy();
        StateHasChanged();
    }

    public async Task CloseModal()
    {
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task GenerateTestDataForAllFields()
    {
        var data = await _properties
            .Where(x => !x.IsKey && !x.IsNavigationProperty && !x.IsForeignKey)
            .GenerateTestData();

        foreach (var item in data)
        {
            InputStringValues[item.Key] = item.Value;
        }

        StateHasChanged();
    }

    private async Task GoBack()
    {
        if (CurrentChildResource != null && !ChildDataResourcesStack.Any())
        {
            CurrentChildResource = null;
            EntityName = RootEntityResource.Name;
        }
        else
        {
            CurrentChildResource = ChildDataResourcesStack.Pop();
            EntityName = CurrentChildResource.Name;
        }

        await OnParametersSetAsync();
        StateHasChanged();
    }

    private async Task CreateRelationEntity(DataProperty propertyInfo)
    {
        EntityName = propertyInfo.EntityPropertyInfo.PropertyType.Name; //Section of Model class type

        await _resourceDialogStrategy.MapStringValuesToEntity();
        if (CurrentChildResource != null)
        {
            ChildDataResourcesStack.Push(CurrentChildResource);
        }

        var entity = Activator.CreateInstance(propertyInfo.EntityPropertyInfo.PropertyType);
        CurrentChildResource = await DataMapperService.MapToTableResource(entity);

        await OnParametersSetAsync();
        StateHasChanged();
    }

    private Task LoadEntityNames()
    {
        _entityNames.Clear();
        foreach (var item in DbInfoService.GetEntityNames())
        {
            _entityNames[item] = item;
        }

        StateHasChanged();
        return Task.CompletedTask;
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
        //TODO improve architecture
        await _resourceDialogStrategy.MapStringValuesToEntity();
        if (CurrentChildResource != null)
        {
            ChildDataResourcesStack.Push(CurrentChildResource);
        }

        EntityName = entityName;
        var entityType =
            await DbInfoService
                .GetEntityType(
                    entityName); //TODO optimize, save in dictionary the entityName and type ` memotization
        var entity = Activator.CreateInstance(entityType);
        CurrentChildResource = await DataMapperService.MapToTableResource(entity);

        await OnParametersSetAsync();

        StateHasChanged();
    }

    private Task OnSelectedRelationProperty(string value, string foreignKeyPropertyName)
    {
        InputStringValues[foreignKeyPropertyName] = value;
        StateHasChanged();

        return Task.CompletedTask;
    }

    #endregion
}