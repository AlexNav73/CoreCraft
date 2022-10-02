using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navitski.Crystalized.Model.Engine;
using WpfDemoApp.Model.Entities;
using WpfDemoApp.ModelCommands;

namespace WpfDemoApp.ViewModels;

internal partial class ItemViewModel : ObservableObject
{
    private readonly UndoRedoDomainModel _model;

    [ObservableProperty]
    private string _name;

    public ItemViewModel(ToDoItem item, ToDoItemProperties props, UndoRedoDomainModel model)
    {
        _model = model;
        _name = props.Name;

        Item = item;
    }

    public ToDoItem Item { get; }

    partial void OnNameChanged(string value)
    {
        var command = new ModifyItemModelCommand(_model);
        command.Entity.Set(Item);
        command.NewName.Set(value);
        command.Execute();
    }

    [RelayCommand]
    private void Remove()
    {
        var command = new RemoveItemModelCommand(_model);
        command.Entity.Set(Item);
        command.Execute();
    }
}
