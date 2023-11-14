using System.Linq.Expressions;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;

namespace DAdmin.Components.Components.ResourceGrid;

public partial class AggregateColumn<TEntity>
{
    [Parameter] public Expression<Func<TEntity, object>> AggregationSelector { get; set; }
    [Parameter] public DataGridAggregateType AggregateType { get; set; }
    [Parameter] public string Field { get; set; }
    [Parameter] public string DisplayFormat { get; set; }
    [Parameter] public IFormatProvider DisplayFormatProvider { get; set; }
    [CascadingParameter] public DataGrid<TEntity> ParentDataGrid { get; set; }
}