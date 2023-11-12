using DAdmin.Components.Components.Charts.Enums;
using Microsoft.AspNetCore.Components;

namespace DAdmin.Components.Components.Charts.ViewModels;

public interface IChart
{
    string Name { get; set; }
    ChartType ChartType { get; set; }
    ChartTimeInterval TimeInterval { get; set; }
    AggregationType AggregationType { get; set; }

    RenderFragment Render();
}