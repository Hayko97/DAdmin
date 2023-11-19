using DAdmin.Charts.Enums;
using Microsoft.AspNetCore.Components;

namespace DAdmin.Charts.ViewModels;

public interface IChart
{
    string Name { get; set; }
    ChartType ChartType { get; set; }
    ChartTimeInterval TimeInterval { get; set; }
    AggregationType AggregationType { get; set; }

    RenderFragment Render();
}