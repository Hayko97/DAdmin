using System.Reflection;
using System.Text;
using Blazorise.DataGrid;
using DAdmin.Dto;
using DAdmin.Menus.ViewModels;
using DAdmin.Helpers;
using DAdmin.Services.DbServices.Interfaces;
using DAdmin.States;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DAdmin
{
    public partial class DataResource<TEntity> : DAdminComponent where TEntity : class
    {
        #region Fields and Properties

        private bool _isEditModalOpen;
        private bool _isCreateModalOpen;
        private string _previousResourceName;
        private string _entityName;

        private List<DataResourceDto<TEntity>>? _data = new();
        private List<TEntity> _dataBlazorise = new(); //TODO Refactor
        private List<DataProperty>? _columns = new();
        private bool _isLoaded;

        private List<TEntity> _selectedRecords = new();
        private TEntity? _selectedRow;

        private DataGridReadDataEventArgs<TEntity> _dataGridReadDataEventArgs;

        private int TotalCount { get; set; }

        [Parameter] public string ResourceName { get; set; }
        [Parameter] public Func<IQueryable<TEntity>, IQueryable<TEntity>>? QueryLogic { get; set; }

        [Parameter] public string[] ExcludedProperties { get; set; }
        [Parameter] public bool UseContextEntities { get; set; } = true;
        [Parameter] public RenderFragment Aggregators { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private MenuState MenuState { get; set; }
        [Inject] private IDataService<TEntity> DataService { get; set; }

        #endregion

        #region Lifecycle Methods

        protected override async Task OnInitializedAsync()
        {
            _isLoaded = false;
            _entityName = typeof(TEntity).Name;
            await LoadTableData();
            _columns = GetColumns(_data)?.ToList();
            _isLoaded = true;
        }

        #endregion

        #region Data Operations

        private async Task LoadTableData()
        {
            try
            {
                var response = await DataService.GetPaginatedAsync(QueryLogic, 1);
                _data = response.Data;
                TotalCount = response.TotalCount;
            }
            catch (Exception ex)
            {
                //await JSRuntime.InvokeVoidAsync("alert", ex.Message);
            }
            finally
            {
                StateHasChanged();
            }
        }

        private IEnumerable<DataProperty>? GetColumns(List<DataResourceDto<TEntity>> resources)
        {
            IEnumerable<DataProperty>? columns = null;

            if (_data != null && _data.Any() && UseContextEntities)
            {
                columns = _data
                    .FirstOrDefault()
                    ?.GetPropertiesWithoutRelations();

                if (ExcludedProperties != null)
                {
                    if (columns != null)
                    {
                        columns = columns.Where(x => ExcludedProperties.All(c => c != x.Name)).ToList();
                    }
                }
            }

            return columns;
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
                }
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
                        await DataService.DeleteAsync(_entityName, item);
                    }

                    await OnReadData(_dataGridReadDataEventArgs); // Refresh data after deletion
                }
                catch (DbUpdateException ex)
                {
                    await JSRuntime.InvokeVoidAsync("alert", ex.Message);
                }
            }
        }

        private string ConvertToCsv(IEnumerable<DataResourceDto<TEntity>> data)
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
            await JsInterop.InvokeVoidAsync("downloadFile", csvContent, "data.csv", "text/csv");
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