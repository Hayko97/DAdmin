using DAdmin.Builders;
using DAdmin.Charts.ViewModels;
using DAdmin.Dto.Stats;
using DAdmin.Services.DbServices.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DAdmin;

public partial class Dashboard : DAdminComponent
{
    [Parameter] public IEnumerable<IChart> Charts { get; set; }

    [Inject] public IDbInfoService DbInfoService { get; set; }

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

            await JsInterop.InvokeVoidAsync("createMultiTableActivityChart", "activityChart", allDates, datasets);
        }
    }
}