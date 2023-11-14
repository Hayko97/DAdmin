using System.Linq.Expressions;
using DAdmin.Components.Components.Charts.Enums;
using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Helpers;
using DAdmin.Components.Services.DbServices.Interfaces;
using DAdmin.Components.States;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DAdmin.Components.Components;

public partial class TimeChart<TEntity>
{
    [Parameter] public ChartType ChartType { get; set; }
    [Parameter] public string Name { get; set; }
    [Parameter] public Func<IQueryable<TEntity>, IQueryable<TEntity>> QueryLogic { get; set; }
    [Parameter] public Expression<Func<TEntity, DateTime>> LabelSelector { get; set; }
    [Parameter] public Expression<Func<TEntity, object>> AggregationSelector { get; set; }

    [Parameter] public ChartTimeInterval TimeInterval { get; set; } = ChartTimeInterval.Daily;
    [Parameter] public AggregationType AggregationType { get; set; } = AggregationType.Count;

    [CascadingParameter] public AdminPanel? AdminPanel { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private IDataService<TEntity> DataService { get; set; }
    [Inject] private MenuState MenuState { get; set; }

    private string canvasId = $"chartCanvas-{Guid.NewGuid()}";
    private bool isChartDataReady = false;
    private ChartData chartData;

    protected override async Task OnInitializedAsync()
    {
        if (QueryLogic != null && LabelSelector != null && AggregationSelector != null)
        {
            var query = QueryLogic(DataService.Query());
            var entities = await query.ToListAsync();
            chartData = ProcessChartData(entities);
            isChartDataReady = true;
        }

        if (AdminPanel != null)
        {
            var parameters = ClassHelper.ExtractParameters(this);

            await MenuState.AddMenuItemAsync(new MenuItemModel
            {
                Section = MenuSection.Charts,
                Name = Name,
                ComponentType = this.GetType(),
                Parameters = parameters,
            });
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (isChartDataReady)
        {
            await RenderChart();
        }
    }

    private ChartData ProcessChartData(List<TEntity> entities)
    {
        var groupedData = GroupDataByTimeInterval(entities);
        var labels = groupedData.Select(g => g.Key).ToList();
        var aggregatedValues = groupedData.Select(g => AggregateValues(g, AggregationType)).ToList();

        return new ChartData
        {
            Labels = labels.ToArray(),
            Datasets = new List<ChartDataset>
            {
                new ChartDataset
                {
                    Label = AggregationType.ToString(),
                    Data = aggregatedValues.ToArray(),
                    BackgroundColor = "rgba(0, 123, 255, 0.5)",
                    BorderColor = "rgba(0, 123, 255, 1)"
                }
            }
        };
    }

    private double AggregateValues(IEnumerable<TEntity> group, AggregationType aggregationType)
    {
        return aggregationType switch
        {
            AggregationType.Sum => group.Sum(entity => Convert.ToDouble(AggregationSelector.Compile()(entity))),
            AggregationType.Average => group.Average(entity => Convert.ToDouble(AggregationSelector.Compile()(entity))),
            AggregationType.Count => group.Count(),
            AggregationType.Max => group.Max(entity => Convert.ToDouble(AggregationSelector.Compile()(entity))),
            AggregationType.Min => group.Min(entity => Convert.ToDouble(AggregationSelector.Compile()(entity))),
            AggregationType.Median => GetMedian(group.Select(entity =>
                Convert.ToDouble(AggregationSelector.Compile()(entity)))),
            _ => group.Count(), // Default or handle appropriately
        };
    }

    private double GetMedian(IEnumerable<double> values)
    {
        var sortedValues = values.OrderBy(x => x).ToList();
        int count = sortedValues.Count;

        if (count == 0)
        {
            return 0; // Or handle empty list appropriately
        }

        double medianValue;
        int midIndex = count / 2;

        if (count % 2 == 0)
        {
            // Even number of items; median is the average of the two central items
            medianValue = (sortedValues[midIndex] + sortedValues[midIndex - 1]) / 2.0;
        }
        else
        {
            // Odd number of items; median is the central item
            medianValue = sortedValues[midIndex];
        }

        return medianValue;
    }

    private IEnumerable<IGrouping<string, TEntity>> GroupDataByTimeInterval(List<TEntity> entities)
    {
        var query = entities.AsQueryable();

        Expression<Func<TEntity, string>> groupExpr = TimeInterval switch
        {
            ChartTimeInterval.Daily => entity => LabelSelector.Compile()(entity).DateToString(),
            ChartTimeInterval.Weekly => entity => LabelSelector.Compile()(entity).WeekOfYear(),
            ChartTimeInterval.Monthly => entity => LabelSelector.Compile()(entity).MonthToString(),
            ChartTimeInterval.Yearly => entity => LabelSelector.Compile()(entity).YearToString(),
            _ => entity => LabelSelector.Compile()(entity).DateToString(),
        };

        return query.GroupBy(groupExpr).ToList();
    }

    private async Task RenderChart()
    {
        var chartOptions = new { }; // Define specific options for the chart

        var jsChartType = ChartType switch
        {
            ChartType.Bar => "bar",
            ChartType.Line => "line",
            ChartType.Pie => "pie",
            ChartType.Doughnut => "doughnut",
            ChartType.Radar => "radar",
            _ => "bar",
        };

        await JSRuntime.InvokeVoidAsync("updateChart", canvasId, jsChartType, chartData, chartOptions);
    }

    private async Task OnTimeIntervalChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<ChartTimeInterval>(e.Value?.ToString(), out var newInterval))
        {
            TimeInterval = newInterval;
            await UpdateChart();
        }
    }

    private async Task UpdateChart()
    {
        var query = QueryLogic(DataService.Query());
        var entities = await query.ToListAsync();
        chartData = ProcessChartData(entities);

        await RenderChart();
    }
}