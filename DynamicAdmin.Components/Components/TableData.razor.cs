using System.Reflection;
using System.Text;
using Blazorise.DataGrid;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components
{
    public partial class TableData<TEntity> where TEntity : class
    {
        #region Fields and Properties

        private bool _isEditModalOpen;
        private bool _isCreateModalOpen;
        private string _previousResourceName;
        private string _searchQuery = string.Empty;
        private int _currentPage = 1;
        private string _entityName;


        private List<Entity<TEntity>> _data = new();
        private List<TEntity> _dataBlazorise = new();

        private List<TEntity> _selectedRecords = new();
        private TEntity _selectedRow;

        private DataGridReadDataEventArgs<TEntity> _dataGridReadDataEventArgs;

        private int TotalPages { get; set; }
        private int TotalCount { get; set; }

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
                if (!string.IsNullOrEmpty(ResourceName) && DataService != null && ResourceName != _previousResourceName)
                {
                    _entityName = typeof(TEntity).Name;
                    await LoadTableData(_currentPage);
                    _previousResourceName = ResourceName;
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
            TotalCount = response.TotalCount;
            StateHasChanged();
        }

        private async Task OnReadData(DataGridReadDataEventArgs<TEntity> e)
        {
            _dataGridReadDataEventArgs = e;
            if (!e.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var response = await DataService.GetPaginatedAsync(QueryLogic, e.Page,
                        e.Columns.Where(x => x.ColumnType == DataGridColumnType.Text));

                    if (!e.CancellationToken.IsCancellationRequested)
                    {
                        TotalCount = response.TotalCount;
                        _dataBlazorise = response.Data.Select(x => x.EntityModel).ToList();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
                // this can be call to anything, in this case we're calling a fictional api
                //var response = await Http.GetJsonAsync<Employee[]>( $"some-api/employees?page={e.Page}&pageSize={e.PageSize}" );
            }
        }

        private async Task ApplySearch()
        {
            _currentPage = 1;
            await LoadTableData(_currentPage);
        }

        private async Task DeleteSelectedItems()
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this item?");
            if (confirmed)
            {
                try
                {
                    foreach (var item in _selectedRecords)
                    {
                        await DataService.DeleteAsync(ResourceName, item);
                    }

                    await OnReadData(_dataGridReadDataEventArgs); // Refresh data after deletion
                }
                catch (DbUpdateException ex)
                {
                    await JSRuntime.InvokeVoidAsync("alert",
                        ex.Message);
                }
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

        private async Task EditSelectedRow(MouseEventArgs e, TEntity item)
        {
            await OpenEditModal(item);
        }

        private async Task OpenEditModal(TEntity item)
        {
            _isEditModalOpen = true;
            _selectedRow = item;
            StateHasChanged();
        }

        private async Task CloseEditModal()
        {
            _isEditModalOpen = false;
        }

        private async Task OnCreateModalSave()
        {
            await OnReadData(_dataGridReadDataEventArgs);
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