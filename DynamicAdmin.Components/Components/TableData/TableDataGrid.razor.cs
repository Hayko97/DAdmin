using Blazorise.DataGrid;
using DynamicAdmin.Components.ViewModels;
using Microsoft.AspNetCore.Components;

namespace DynamicAdmin.Components.Components.TableData;

public partial class TableDataGrid<TEntity>
{
    [Parameter] public IEnumerable<Entity<TEntity>> Data { get; set; }
    [Parameter] public IEnumerable<TEntity> DataBlazorise { get; set; }

    [Parameter] public int CurrentPage { get; set; }

    [Parameter] public int TotalPages { get; set; }
    [Parameter] public int TotalCount { get; set; }

    [Parameter] public EventCallback<List<TEntity>> OnEdit { get; set; }

    [Parameter] public EventCallback<List<TEntity>> OnDelete { get; set; }
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }
    [Parameter] public EventCallback<DataGridReadDataEventArgs<TEntity>> OnReadTableData { get; set; }

    private List<TEntity> _selectedRecords;
    private TEntity _selectedRow;

    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await RefreshData(); // Method to refresh data based on the new page
        }
    }

    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await RefreshData(); // Method to refresh data based on the new page
        }
    }

    private async Task OnReadData(DataGridReadDataEventArgs<TEntity> e)
    {
        await OnReadTableData.InvokeAsync(e);
    }

    private async Task GoToPage(int page)
    {
        CurrentPage = page;
        await RefreshData(); // Method to refresh data based on the new page
    }

    private async Task RefreshData()
    {
        await OnPageChanged.InvokeAsync(CurrentPage);
    }
}