using CommunityToolkit.Mvvm.ComponentModel;
using CoreCraft;
using CoreCraft.Generators.Bindings;
using CoreCraft.Subscription.Extensions;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ViewModels;

internal partial class ToDoItemViewModel : ObservableObject
{
    private readonly UndoRedoDomainModel _model;

    [ObservableProperty]
    [property: Binding(nameof(ToDoItem), nameof(ToDoItemProperties.IsChecked))]
    private bool _isChecked;
    [ObservableProperty]
    [property: Binding(nameof(ToDoItem), nameof(ToDoItemProperties.Name))]
    private string _name;

    public ToDoItemViewModel(ToDoItem item, ToDoItemProperties props, UndoRedoDomainModel model)
        : this(item)
    {
        _model = model;
        _name = props.Name;
        _isChecked = props.IsChecked;

        _model.For<IToDoChangesFrame>()
            .With(x => x.Items)
            .Bind(this);
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
