using System.Reflection;
using System.Text;
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
        private string _entityName;

        private List<Entity<TEntity>> _data = new();
        private Entity<TEntity> _selectedItem;
        private int TotalPages { get; set; }

        [Parameter] public string ResourceName { get; set; }
        [Parameter] public Func<IQueryable<TEntity>, IQueryable<TEntity>> QueryLogic { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private IDataService<TEntity> DataService { get; set; }

        #endregion

        #region Lifecycle Methods

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(ResourceName) && DataService != null && ResourceName != _previousTableName)
                {
                    _entityName = typeof(TEntity).Name;
                    await LoadTableData(_currentPage);
                    _previousTableName = ResourceName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnParametersSet: {ex.Message}");
            }
        }

        #endregion

        #region Data Operations

        private async Task LoadTableData(int currentPage)
        {
            _currentPage = currentPage;
            var response = await DataService.GetPaginatedAsync(QueryLogic, currentPage, _searchQuery);
            _data = response.Data;
            TotalPages = response.TotalPages;
            StateHasChanged();
        }

        private async Task ApplySearch()
        {
            _currentPage = 1;
            await LoadTableData(_currentPage);
        }

        private async Task DeleteItem(Entity<TEntity> item)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this item?");
            if (confirmed)
            {
                await DataService.DeleteAsync(ResourceName, item.EntityModel);
                await LoadTableData(_currentPage); // Refresh data after deletion
            }
        }

        private string ConvertToCsv(IEnumerable<Entity<TEntity>> data)
        {
            var csvBuilder = new StringBuilder();
            var properties = data.FirstOrDefault().GetPropertiesWithoutRelations();

            // Adding header
            csvBuilder.AppendLine(string.Join(",", properties.Select(name => name.Name.ToString())));

            // Adding data
            foreach (var item in data)
            {
                var line = string.Join(",", item.Properties.Select(p => p.Value));
                csvBuilder.AppendLine(line);
            }

            return csvBuilder.ToString();
        }

        private async Task DownloadCsv()
        {
            var csvContent = ConvertToCsv(_data);
            await JSRuntime.InvokeVoidAsync("downloadFile", csvContent, "data.csv", "text/csv");
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
            await LoadTableData(_currentPage);
            StateHasChanged();
        }

        private async Task OnEditModalSave()
        {
            await LoadTableData(_currentPage);
            StateHasChanged();
        }

        #endregion
    }
}