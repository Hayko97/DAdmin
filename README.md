# DAdmin Component Library

## Introduction
This library provides a suite of Blazor components designed to create powerful and visually appealing admin panels with ease. It leverages EF DbContext and its entities to automate much of the admin panel creation. Key components include `AdminPanel`, `MenuItem`, `DataResource`, `AggregateColumn`, and `TimeChart`.

## Installation

### .NET CLI
To install the package using the .NET CLI, use the following command:
```shell
dotnet add package dadmin
```
### NuGet Package Manager Console
```shell
Install-Package dadmin
```

## Getting Started
To use these components, first ensure that you have referenced this library in your Blazor application. The library uses Entity Framework (EF) DbContext and its entities to generate data grids and other components dynamically.

## Components

### AdminPanel
The `AdminPanel` component is the container for your administrative interface. It can automatically use DbContext entities to create a data grid for all DbContext entities when `UseContextEntities` is set to `true`.

#### Usage
- Basic Usage: `<AdminPanel/>` creates a default admin panel without automatic DbContext integration.
- Advanced Usage: Set `UseContextEntities="true"` to enable automatic generation of data grids for each entity in your DbContext.

```html
<AdminPanel UseContextEntities="true">
    <MenuItem Name="Transactions" MenuSection="MenuSection.Resources" IconClass="fa fa-table">
        <DataResource TEntity="Transaction" ResourceName="Transactions" ExcludedProperties="@(new[] { "Status", "Username" })">
        <AggregateColumn TEntity="Transaction" AggregationSelector="x => x.Amount" Aggregate="DataGridAggregateType.Sum"/>
        </DataResource>
    </MenuItem>
    <MenuItem Name="Transactions" MenuSection="MenuSection.Charts" IconClass="fa fa-table">
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
### AggregateColumn
AggregateColumn is used within DataResource for aggregating data like sums or averages.

#### Usage
``` html
<AggregateColumn TEntity="Transaction" AggregationSelector="x => x.Amount" Aggregate="DataGridAggregateType.Sum"/>
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

