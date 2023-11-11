using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicAdmin.Components.Components.EntityDialog.Strategies;
using DynamicAdmin.Components.Extensions;
using Microsoft.AspNetCore.Components;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;
using Microsoft.JSInterop;

namespace DynamicAdmin.Components.Components
{
    public partial class CreateEntityDialog<TEntity> where TEntity : class
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public string EntityName { get; set; }
        [Parameter] public EventCallback<TEntity> OnSave { get; set; }
        [Parameter] public EventCallback OnCloseModal { get; set; }
        [Inject] public IDataService<TEntity> DataService { get; set; }
        [Inject] public IDataMapperService<TEntity> DataMapperService { get; set; }
        [Inject] public IDbInfoService DbInfoService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        public Entity<TEntity>? RootEntity { get; set; }
        public Entity<object>? ObjectEntity { get; set; }

        private IEnumerable<EntityProperty> _properties;

        private IEntityDialogStrategy _entityDialogStrategy;
        public Dictionary<string, object> InputValues { get; set; } = new();
        public Dictionary<string, string> InputStringValues { get; set; } = new();

        private Dictionary<string, string> _entityNames = new();

        public Stack<Entity<object>> EntityObjectStack { get; set; } = new();

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
            _entityDialogStrategy = ObjectEntity?.Properties == null
                ? new EntityRootDialogStrategy<TEntity>(this)
                : new EntityObjectDialogStrategy<TEntity>(this);

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

        private async Task CreateEntity()
        {
            try
            {
                await Task.Delay(400);
                await _entityDialogStrategy.CreateEntity();
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
            if (ObjectEntity != null && !EntityObjectStack.Any())
            {
                ObjectEntity = null;
                EntityName = RootEntity.Name;
            }
            else
            {
                ObjectEntity = EntityObjectStack.Pop();
                EntityName = ObjectEntity.Name;
            }

            await OnParametersSetAsync();
            StateHasChanged();
        }

        private async Task CreateRelationEntity(EntityProperty propertyInfo)
        {
            EntityName = propertyInfo.TablePropertyInfo.PropertyType.Name; //Name of Model class type

            await _entityDialogStrategy.MapStringValuesToEntity();
            if (ObjectEntity != null)
            {
                EntityObjectStack.Push(ObjectEntity);
            }

            var entity = Activator.CreateInstance(propertyInfo.TablePropertyInfo.PropertyType);
            ObjectEntity = await DataMapperService.GetEntityViewModel(entity);

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
            if (ObjectEntity != null)
            {
                EntityObjectStack.Push(ObjectEntity);
            }

            EntityName = entityName;
            var entityType =
                await DbInfoService
                    .GetEntityType(
                        entityName); //TODO optimize, save in dictionary the entityName and type ` memotization
            var entity = Activator.CreateInstance(entityType);
            ObjectEntity = await DataMapperService.GetEntityViewModel(entity);

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