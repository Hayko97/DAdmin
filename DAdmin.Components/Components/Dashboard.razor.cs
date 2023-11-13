using DAdmin.Components.Builders;
using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Services;
using DAdmin.Components.Services.Interfaces;
using DAdmin.Shared.DTO.Stats;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DAdmin.Components.Components;

public partial class Dashboard
{
    [Parameter] public IEnumerable<IChart> Charts { get; set; }

    [Inject] public IDbInfoService DbInfoService { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    private Stats _stats;
    private StatsBuilder _statsBuilder;
    private bool _isStatsLoaded;

    protected override async Task OnInitializedAsync()
    {
        _statsBuilder = DbInfoService.GetStatsBuilder();
        _statsBuilder = await _statsBuilder.WithGeneralStatsAsync();
        _statsBuilder = await _statsBuilder.WithErrorStatsAsync();
        _statsBuilder = await _statsBuilder.WithSalesStatsAsync();
        _stats = _statsBuilder.Build();

        _isStatsLoaded = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_stats != null && _stats.ActivityPerTableOverTime != null)
        {
            var allDates = _stats.ActivityPerTableOverTime
                .SelectMany(x => x.Value.Keys)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => d.ToShortDateString())
                .ToArray();

            var datasets = _stats.ActivityPerTableOverTime.Select(tableActivity =>
            {
                var tableName = tableActivity.Key;
                var counts = allDates.Select(date => 
                {
                    var activityDate = DateTime.Parse(date);
                    return tableActivity.Value.TryGetValue(activityDate, out var count) ? count : 0;
                }).ToArray();

                return new { Label = tableName, Data = counts };
            }).ToList();

            await JSRuntime.InvokeVoidAsync("createMultiTableActivityChart", "activityChart", allDates, datasets);
        }
    }
}