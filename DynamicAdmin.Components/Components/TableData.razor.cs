using System.Reflection;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components
{
    public partial class TableData<TEntity> where TEntity : class
    {
        #region Fields and Properties

        private bool _isEditModalOpen;
        private bool _isCreateModalOpen;
        private string _previousTableName;
        private string _searchQuery = string.Empty;
        private int _currentPage = 1;
        
        private List<Entity<TEntity>> _data = new();
        private Entity<TEntity> _selectedItem;

        private bool IsFirstPage => _currentPage == 1;
        private bool IsLastPage => _currentPage == TotalPages;
        private int TotalPages { get; set; }

        [Parameter] public string EntityName { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private IDataService<TEntity> DataService { get; set; }

        #endregion

        #region Lifecycle Methods

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(EntityName) && DataService != null && EntityName != _previousTableName)
                {
                    await LoadTableData();
                    _previousTableName = EntityName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnParametersSet: {ex.Message}");
            }
        }

        #endregion

        #region Data Operations

        private async Task LoadTableData()
        {
            var response = await DataService.GetPaginatedAsync(EntityName, _currentPage, _searchQuery);
            _data = response.Data;
            TotalPages = response.TotalPages;
            StateHasChanged();
        }

        private async Task ApplySearch()
        {
            _currentPage = 1;
            await LoadTableData();
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

        private async Task DeleteItem(Entity<TEntity> item)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this item?");
            if (confirmed)
            {
                await DataService.DeleteAsync(EntityName, item.EntityModel);
                await LoadTableData(); // Refresh data after deletion
            }
        }

        #endregion

        #region Modal Operations

        private async Task OpenCreateModal()
        {
            _isCreateModalOpen = true;
        }

        private async Task CloseCreateModal()
        {
            _isCreateModalOpen = false;
        }

        private async Task OpenEditModal(Entity<TEntity> item)
        {
            _selectedItem = item;
            _isEditModalOpen = true;
            StateHasChanged();
        }

        private async Task CloseEditModal()
        {
            _isEditModalOpen = false;
            _selectedItem = default;
        }

        private async Task EditItem(Entity<TEntity> item)
        {
            await OpenEditModal(item);
        }

        private async Task OnCreateModalSave()
        {
            await LoadTableData();
            StateHasChanged();
        }

        private async Task OnEditModalSave()
        {
            await LoadTableData();
            StateHasChanged();
        }

        #endregion
    }
}
