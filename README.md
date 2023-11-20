# DAdmin Component Library

## Introduction
This library provides a suite of Blazor components designed to create powerful and visually appealing admin panels with ease. It leverages EF DbContext and its entities to automate much of the admin panel creation. Key components include `AdminPanel`, `MenuItem`, `DataResource`, `Aggregator`, and `TimeChart`.

## Getting Started
To use these components, first ensure that you have referenced this library in your Blazor Server application. The library uses Entity Framework (EF) DbContext and its entities to generate data grids and other components dynamically.

#### Program.cs
```c#
builder.Services.DAdmin<YourDbContext>();
```

#### _imports.razor

```c#
@using DAdmin
```
## Components

### AdminPanel
The `AdminPanel` component is the container for your administrative interface. It can automatically use DbContext entities to create a data grid for all DbContext entities when `UseContextEntities` is set to `true`.

#### Usage
- Basic Usage: `<AdminPanel/>` creates a default admin panel using integrated EF DbContext.
- Advanced Usage: Set `UseContextEntities="true"` to enable automatic generation of data grids for each entity in your DbContext.

```html
<AdminPanel UseContextEntities="true">
    <MenuItem Name="Transactions" MenuSection="MenuSection.Resources">
        <DataResource TEntity="Transaction" ExcludedProperties="@(new[] { "Status", "Username" })">
        <Aggregators>
            <Aggregator TEntity="Transaction" AggregationSelector="x => x.Amount" Aggregate="DataGridAggregateType.Sum"/>
        </Aggregators>
        </DataResource>
    </MenuItem>
    <MenuItem Name="Transactions Charts" MenuSection="MenuSection.Charts">
        <TimeChart TEntity="Transaction"
                   Name="Transaction Activity"
                   ChartType="ChartType.Line"
                   AggregationType="AggregationType.Count"
                   AggregationSelector="entity => entity.Id"
                   LabelSelector="entity => entity.CreatedAt"
                   TimeInterval="ChartTimeInterval.Monthly"
                   QueryLogic="query => query"/>
    </MenuItem>
</AdminPanel>
```

### MenuItem
MenuItem defines an individual item in the admin panel, typically linked to a data resource.

#### Usage
``` html
<MenuItem Name="Transactions" MenuSection="MenuSection.Resources" IconClass="fa fa-table">
    <!-- Data resource here -->
</MenuItem>
```
### DataResource
DataResource connects a data entity to a menu item, allowing for data-driven components within the admin panel.

#### Usage
```html
<DataResource TEntity="Transaction" ResourceName="Transactions" ExcludedProperties="@(new[] { "Status", "Username" })">
<!-- Aggregate column or other elements -->
</DataResource>
```
### Aggregator
Aggregator is used within DataResource for aggregating data like sums or averages.

#### Usage
``` html
<Aggregator TEntity="Transaction" AggregationSelector="x => x.Amount" Aggregate="DataGridAggregateType.Sum"/>
```

### TimeChart

TimeChart provides a way to visualize data over time, such as line charts or bar graphs.

#### Usage
``` html
<TimeChart TEntity="Transaction"
           Name="Transaction Activity"
           ChartType="ChartType.Line"
           AggregationType="AggregationType.Count"
           AggregationSelector="entity => entity.Id"
           LabelSelector="entity => entity.CreatedAt"
           TimeInterval="ChartTimeInterval.Monthly"
           QueryLogic="query => query"/>
```

We welcome contributions to this library. Please follow the standard procedures for submitting issues and pull requests.

