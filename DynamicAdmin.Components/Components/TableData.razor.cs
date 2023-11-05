using System.Reflection;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Models;
using DynamicAdmin.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components;

public partial class TableData<TEntity> where TEntity : class
{
    private bool _isEditModalOpen;
    private bool _isCreateModalOpen;

    private string _previousTableName;

    #region Data And Pagination

    private Type _entityType;
    private string _searchQuery = string.Empty;
    private List<EntityViewModel<TEntity>> _data = new();

    private int _currentPage = 1;
    private int TotalPages { get; set; }

    private bool IsFirstPage => _currentPage == 1;
    private bool IsLastPage => _currentPage == TotalPages;

    #endregion

    #region Form and data

    private EntityViewModel<TEntity> _selectedItem;
    private EntityViewModel<TEntity> _newItem;
    private Dictionary<string, object> _selectedItemInputValues = new();
    private Dictionary<string, string> _selectedStringValues = new();
    private Dictionary<string, object> _newItemInputValues = new();
    private Dictionary<string, string> _newStringValues = new();

    #endregion

    [Parameter] public string TableName { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IDataService<TEntity> DataService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(TableName) && DataService != null && TableName != _previousTableName)
            {
                ResetVariables();
                await LoadTableData();
                _previousTableName = TableName;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnParametersSet: {ex.Message}");
        }
    }

    private async Task ApplySearch()
    {
        _currentPage = 1;
        await LoadTableData();
    }

    private async Task LoadTableData()
    {
        var response = await DataService.GetPaginatedAsync(TableName, _currentPage, _searchQuery);
        _data = response.Data;
        TotalPages = response.TotalPages;
        StateHasChanged();
    }

    private async Task NextPage()
    {
        if (!IsLastPage)
        {
            _currentPage++;
            await LoadTableData();
        }
    }

    private async Task PreviousPage()
    {
        if (!IsFirstPage)
        {
            _currentPage--;
            await LoadTableData();
        }
    }

    private async void GoToPage(int pageNumber)
    {
        _currentPage = pageNumber;
        await LoadTableData();
    }

    private async Task OpenCreateModal()
    {
        _isCreateModalOpen = true;
    }

    private async Task CloseCreateModal()
    {
        _isCreateModalOpen = false;
        _newItem = default;
    }

    private async Task OpenEditModal(EntityViewModel<TEntity> item)
    {
        _selectedItem = await DataService.GetEntityViewModel(item.Entity);
        _selectedItemInputValues.Clear();
        _selectedStringValues.Clear();

        foreach (var prop in _selectedItem.Properties)
        {
            if (!_selectedItemInputValues.ContainsKey(prop.Name))
            {
                _selectedItemInputValues[prop.Name] = prop.Value;
            }

            if (!_selectedStringValues.ContainsKey(prop.Name))
            {
                _selectedStringValues[prop.Name] = _selectedItemInputValues[prop.Name]?.ToString();
            }
        }

        _isEditModalOpen = true;
    }

    private async Task CloseModal()
    {
        _isEditModalOpen = false;
        _selectedItem = default;
    }

    private async Task CreateEntity()
    {
        await LoadTableData();
        StateHasChanged();
    }

    private async Task EditSelectedItem()
    {
        try
        {
            if (_selectedItem == null)
            {
                await JSRuntime.InvokeVoidAsync("alert", "No item selected for editing.");
                return;
            }

            foreach (var prop in _selectedItem.GetPropertiesWithoutRelations())
            {
                if (_selectedItemInputValues.ContainsKey(prop.Name))
                {
                    ClassHelper.SetStringValue(_selectedStringValues[prop.Name], prop.TablePropertyInfo,
                        _selectedItem.Entity);
                }
            }

            await DataService.UpdateAsync(TableName, _selectedItem.Entity);
            await LoadTableData();
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error in SaveChanges: {ex.Message}");
        }
        finally
        {
            CloseModal();
            StateHasChanged();
        }
    }

    private async Task DeleteItem(EntityViewModel<TEntity> item)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this item?");
        if (confirmed)
        {
            await DataService.DeleteAsync(TableName, item.Entity);
            await LoadTableData(); // Refresh data after deletion
        }
    }

    private async Task EditItem(EntityViewModel<TEntity> item)
    {
        await OpenEditModal(item);
    }
    
    private void ResetVariables()
    {
        _selectedItem = default;
        _newItem = default;
        _selectedItemInputValues = new();
        _selectedStringValues = new();
        _newItemInputValues = new();
        _newStringValues = new();
    }
}