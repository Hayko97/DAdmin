using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAdmin.Components.Components.EntityDialog;
using DAdmin.Components.Components.EntityDialog.Strategies;
using DAdmin.Components.Extensions;
using Microsoft.AspNetCore.Components;
using DAdmin.Components.Helpers;
using DAdmin.Components.Services.Interfaces;
using DAdmin.Shared.DTO;
using Microsoft.JSInterop;

namespace DAdmin.Components.Components
{
    public partial class EntityActionDialog<TEntity> where TEntity : class
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public string EntityName { get; set; }
        [Parameter] public DialogMode DialogMode { get; set; }

        [Parameter] public TEntity? SelectedEntity { get; set; }
        [Parameter] public EventCallback<TEntity> OnSave { get; set; }
        [Parameter] public EventCallback OnCloseModal { get; set; }
        [Inject] public IDataService<TEntity> DataService { get; set; }
        [Inject] public IDataMapperService<TEntity> DataMapperService { get; set; }
        [Inject] public IDbInfoService DbInfoService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        public TableResource<TEntity>? RootTableResource { get; set; }
        public TableResource<object>? ObjectTableResource { get; set; }

        private IEnumerable<ResourceProperty> _properties;

        private IEntityDialogStrategy _entityDialogStrategy;
        public Dictionary<string, object> InputValues { get; set; } = new();
        public Dictionary<string, string> InputStringValues { get; set; } = new();

        private Dictionary<string, string> _entityNames = new();

        public Stack<TableResource<object>> TableResourcesStack { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadTableNames();
        }

        protected override async Task OnParametersSetAsync()
        {
            await InitializeStrategy();
        }

        public async Task InitializeStrategy()
        {
            if (ObjectTableResource?.Properties == null && DialogMode == DialogMode.Create)
            {
                _entityDialogStrategy = new CreateRootStrategy<TEntity>(this);
            }
            else if (ObjectTableResource?.Properties != null && DialogMode == DialogMode.Create)
            {
                _entityDialogStrategy = new CreateObjectStrategy<TEntity>(this);
            }
            else if (ObjectTableResource?.Properties == null && DialogMode == DialogMode.Edit)
            {
                _entityDialogStrategy = new EditRootStrategy<TEntity>(this, SelectedEntity);
            }
            else if (ObjectTableResource?.Properties != null && DialogMode == DialogMode.Edit)
            {
                _entityDialogStrategy = new EditRootStrategy<TEntity>(this, SelectedEntity);
            }
            else
            {
                throw new ArgumentException("Invalid strategy");
            }

            _properties = await _entityDialogStrategy.GetProperties();
            PrepareInputValues();
            await _entityDialogStrategy.MapStringValuesToEntity();
        }

        private void PrepareInputValues()
        {
            InputValues.Clear();
            InputStringValues.Clear();

            foreach (var prop in _properties)
            {
                InputValues.TryAdd(prop.Name, prop.Value);
                InputStringValues.TryAdd(prop.Name, prop.IsDefaultValue ? string.Empty : prop.Value?.ToString());
            }
        }

        private async Task Save()
        {
            try
            {
                await Task.Delay(400);
                await _entityDialogStrategy.Save();
                EntityName = _entityDialogStrategy.EntityName;
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Error while saving: {ex.Message}");
            }

            await InitializeStrategy();
            StateHasChanged();
        }

        public async Task CloseModal()
        {
            await IsOpenChanged.InvokeAsync(false);
        }

        private async Task GenerateTestDataForAllFields()
        {
            var data = await _properties
                .Where(x => !x.IsKey && !x.IsNavigationProperty && !x.IsForeignKey)
                .GenerateTestData();

            foreach (var item in data)
            {
                InputStringValues[item.Key] = item.Value;
            }

            StateHasChanged();
        }

        private async Task GoBack()
        {
            if (ObjectTableResource != null && !TableResourcesStack.Any())
            {
                ObjectTableResource = null;
                EntityName = RootTableResource.Name;
            }
            else
            {
                ObjectTableResource = TableResourcesStack.Pop();
                EntityName = ObjectTableResource.Name;
            }

            await OnParametersSetAsync();
            StateHasChanged();
        }

        private async Task CreateRelationEntity(ResourceProperty propertyInfo)
        {
            EntityName = propertyInfo.TablePropertyInfo.PropertyType.Name; //Type of Model class type

            await _entityDialogStrategy.MapStringValuesToEntity();
            if (ObjectTableResource != null)
            {
                TableResourcesStack.Push(ObjectTableResource);
            }

            var entity = Activator.CreateInstance(propertyInfo.TablePropertyInfo.PropertyType);
            ObjectTableResource = await DataMapperService.MapToTableResource(entity);

            await OnParametersSetAsync();
            StateHasChanged();
        }

        private Task LoadEntityNames()
        {
            _entityNames.Clear();
            foreach (var item in DbInfoService.GetEntityNames())
            {
                _entityNames[item] = item;
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task LoadTableNames()
        {
            _entityNames.Clear();
            foreach (var item in DbInfoService.GetEntityNames())
            {
                _entityNames[item] = item;
            }

            StateHasChanged();

            return Task.CompletedTask;
        }

        private async Task OnSelectedEntityName(string entityName)
        {
            //TODO improve architecture
            await _entityDialogStrategy.MapStringValuesToEntity();
            if (ObjectTableResource != null)
            {
                TableResourcesStack.Push(ObjectTableResource);
            }

            EntityName = entityName;
            var entityType =
                await DbInfoService
                    .GetEntityType(
                        entityName); //TODO optimize, save in dictionary the entityName and type ` memotization
            var entity = Activator.CreateInstance(entityType);
            ObjectTableResource = await DataMapperService.MapToTableResource(entity);

            await OnParametersSetAsync();

            StateHasChanged();
        }

        private Task OnSelectedRelationProperty(string value, string foreignKeyPropertyName)
        {
            InputStringValues[foreignKeyPropertyName] = value;
            StateHasChanged();

            return Task.CompletedTask;
        }
    }
}