using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ViewModels;

internal partial class ItemViewModel : ObservableObject, IObserver<IEntityChange<ToDoItem, ToDoItemProperties>>
{
    private readonly UndoRedoDomainModel _model;

    [ObservableProperty]
    private string _name;

    public ItemViewModel(ToDoItem item, ToDoItemProperties props, UndoRedoDomainModel model)
    {
        _model = model;
        _name = props.Name;

        Entity = item;
    }

    public ToDoItem Entity { get; }

    public void OnNext(IEntityChange<ToDoItem, ToDoItemProperties> value)
    {
        Name = value.NewData!.Name;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    partial void OnNameChanged(string value)
    {
        var _ = _model.Run<IMutableToDoModelShard>(
            (shard, _) => shard.Items.Modify(Entity, p => p with { Name = value }));
    }

    [RelayCommand]
    private async Task Remove()
    {
        await _model.Run<IMutableToDoModelShard>((shard, _) => shard.Items.Remove(Entity));
    }
}
