using Microsoft.AspNetCore.Components;

namespace DynamicAdmin.Components.Components.ModalDialogs;

public partial class EditEntity<TEntity> {
    [Parameter] public TEntity SelectedItem { get; set; }
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback OnSaveChanges { get; set; }
    [Parameter] public EventCallback OnCloseModal { get; set; }
    
}
