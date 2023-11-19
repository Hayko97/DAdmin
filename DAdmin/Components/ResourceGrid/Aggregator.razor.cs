using System.Linq.Expressions;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;

namespace DAdmin;

public partial class Aggregator<TEntity>
{
    private string _field = string.Empty;
    [Parameter] public DataGridAggregateType Aggregate { get; set; }
    [Parameter] public Expression<Func<TEntity, object>> AggregationSelector { get; set; }
    [Parameter] public Func<IEnumerable<TEntity>, DataGridColumn<TEntity>, object> AggregationFunction { get; set; }

    [Parameter] public RenderFragment<AggregateContext<TEntity>> DisplayTemplate { get; set; }
    [Parameter] public string DisplayFormat { get; set; }
    [Parameter] public IFormatProvider DisplayFormatProvider { get; set; }

    [CascadingParameter] public DataGrid<TEntity> ParentDataGrid { get; set; }

    protected override void OnInitialized()
    {
        _field = GetPropertyName(AggregationSelector);

        // Configure the aggregation function based on the AggregationSelector
        AggregationFunction = (data, column) => AggregateData(ParentDataGrid.Data, column);
    }

    private object AggregateData(IEnumerable<TEntity> data, DataGridColumn<TEntity> column)
    {
        // Check if the data is null
        if (data == null || AggregationSelector == null)
            return null;

        // Find the column that matches the property name
        var matchingColumn = ParentDataGrid.GetColumns().FirstOrDefault(c => c.Field == _field);
        if (matchingColumn == null)
            return null;

        // Based on the Aggregate type, perform the aggregation
        switch (Aggregate)
        {
            case DataGridAggregateType.Sum:
                return DataGridAggregate<TEntity>.Sum(data, matchingColumn);
            case DataGridAggregateType.Average:
                return DataGridAggregate<TEntity>.Average(data, matchingColumn);
            case DataGridAggregateType.Min:
                return DataGridAggregate<TEntity>.Min(data, matchingColumn);
            case DataGridAggregateType.Max:
                return DataGridAggregate<TEntity>.Max(data, matchingColumn);
            case DataGridAggregateType.Count:
                return DataGridAggregate<TEntity>.Count(data, matchingColumn);
            case DataGridAggregateType.TrueCount:
                return DataGridAggregate<TEntity>.TrueCount(data, matchingColumn);
            case DataGridAggregateType.FalseCount:
                return DataGridAggregate<TEntity>.FalseCount(data, matchingColumn);
            default:
                return null;
        }
    }

    private object RenderAggregateResult()
    {
        var data = ParentDataGrid.Data;
        var column = ParentDataGrid.GetColumns().FirstOrDefault(c => c.Field == _field);

        if (column == null)
            return "N/A";

        return AggregationFunction.Invoke(data, column);
    }

    public string GetPropertyName(Expression<Func<TEntity, object>> expression)
    {
        var member = expression.Body as MemberExpression;
        if (member == null)
        {
            var unary = expression.Body as UnaryExpression;
            if (unary != null)
            {
                member = unary.Operand as MemberExpression;
            }
        }

        return member != null ? member.Member.Name : null;
    }
}