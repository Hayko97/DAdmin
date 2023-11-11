namespace DynamicAdmin.Components.Components.Charts.ViewModels;

using Enums;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

public class Chart<TEntity> : IChart where TEntity : class
{
    public string Name { get; set; }
    public ChartType ChartType { get; set; }
    public Func<IQueryable<TEntity>, IQueryable<TEntity>> QueryLogic { get; set; }
    public Expression<Func<TEntity, DateTime>> LabelSelector { get; set; }
    public Expression<Func<TEntity, object>> AggregationSelector { get; set; }
    public ChartTimeInterval TimeInterval { get; set; } = ChartTimeInterval.Daily;
    public AggregationType AggregationType { get; set; } = AggregationType.Count;

    public RenderFragment Render() => builder =>
    {
        builder.OpenComponent<IntervalChart<TEntity>>(0);
        builder.AddAttribute(1, "ChartType", ChartType);
        builder.AddAttribute(2, "Name", Name);
        builder.AddAttribute(3, "QueryLogic", QueryLogic);
        builder.AddAttribute(4, "LabelSelector", LabelSelector);
        builder.AddAttribute(5, "AggregationSelector", AggregationSelector);
        builder.AddAttribute(6, "TimeInterval", TimeInterval);
        builder.AddAttribute(7, "AggregationType", AggregationType);
        builder.CloseComponent();
    };
}