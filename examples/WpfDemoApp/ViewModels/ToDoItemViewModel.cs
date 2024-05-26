using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCraft;
using CoreCraft.ChangesTracking;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ViewModels;

internal partial class ToDoItemViewModel : ObservableObject
{
    private readonly UndoRedoDomainModel _model;

    [ObservableProperty]
    private bool _isChecked;
    [ObservableProperty]
    private string _name;

    public ToDoItemViewModel(ToDoItem item, ToDoItemProperties props, UndoRedoDomainModel model)
    {
        _model = model;
        _name = props.Name;
        _isChecked = props.IsChecked;

        Entity = item;

        _model.For<IToDoChangesFrame>()
            .With(x => x.Items)
            .Bind(item, Observer.Create<IEntityChange<ToDoItem, ToDoItemProperties>>(OnItemChanged));
    }

    public ToDoItem Entity { get; }

    public void OnItemChanged(IEntityChange<ToDoItem, ToDoItemProperties> value)
    {
        Name = value.NewData!.Name;
        IsChecked = value.NewData!.IsChecked;
    }

    partial void OnNameChanged(string value)
    {
        var _ = _model.Run<IMutableToDoModelShard>(
            (shard, _) => shard.Items.Modify(Entity, p => p with { Name = value }));
    }

    partial void OnIsCheckedChanged(bool value)
    {
        var _ = _model.Run<IMutableToDoModelShard>(
            (shard, _) => shard.Items.Modify(Entity, p => p with { IsChecked = value }));
    }
}
