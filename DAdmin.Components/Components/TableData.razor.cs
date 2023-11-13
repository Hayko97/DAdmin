using System.Reflection;
using System.Text;
using Blazorise.DataGrid;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Helpers;
using DAdmin.Components.Services;
using DAdmin.Components.Services.Interfaces;
using DAdmin.Shared.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DAdmin.Components.Components
{
    public partial class TableData<TEntity> where TEntity : class
    {
        #region Fields and Properties

        private bool _isEditModalOpen;
        private bool _isCreateModalOpen;
        private string _previousResourceName;
        private string _entityName;


        private List<TableResource<TEntity>> _data = new();
        private List<TEntity> _dataBlazorise = new();

        private List<TEntity> _selectedRecords = new();
        private TEntity? _selectedRow;

        private DataGridReadDataEventArgs<TEntity> _dataGridReadDataEventArgs;

        private int TotalCount { get; set; }

        [Parameter] public string ResourceName { get; set; }
        [Parameter] public Func<IQueryable<TEntity>, IQueryable<TEntity>> QueryLogic { get; set; }

        [CascadingParameter] public AdminPanel? AdminPanel { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private IDataService<TEntity> DataService { get; set; }

        #endregion

        #region Lifecycle Methods

        protected override Task OnInitializedAsync()
        {
            if (AdminPanel != null)
            {
                var parameters = ClassHelper.ExtractParameters(this);
                
                AdminPanel.AddMenuItem(new MenuItem()
                {
                    Type = MenuType.Resources,
                    Name = ResourceName,
                    ComponentType = this.GetType(),
                    Parameters = parameters,
                    SubItems = null
                });
                
            }

            return Task.CompletedTask;
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(ResourceName) && DataService != null && ResourceName != _previousResourceName)
                {
                    _entityName = typeof(TEntity).Name;
                    await LoadTableData();
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

        private async Task LoadTableData()
        {
            var response = await DataService.GetPaginatedAsync(QueryLogic, 1);
            _data = response.Data;
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
                        _data = response.Data;
                        _dataBlazorise = response.Data.Select(x => x.EntityModel).ToList();
                    }
                }
                catch (Exception ex)
                {
                    await JSRuntime.InvokeVoidAsync("alert", ex.Message);
                    throw;
                }
                // this can be call to anything, in this case we're calling a fictional api
                //var response = await Http.GetJsonAsync<Employee[]>( $"some-api/employees?page={e.Page}&pageSize={e.PageSize}" );
            }
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

        private string ConvertToCsv(IEnumerable<TableResource<TEntity>> data)
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
            await OnReadData(_dataGridReadDataEventArgs);
            StateHasChanged();
        }

        #endregion
    }
}