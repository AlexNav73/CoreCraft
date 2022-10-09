using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navitski.Crystalized.Model.Engine;
using System.Threading.Tasks;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

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
        _model.Run<IMutableToDoModelShard>(
            (shard, _) => shard.Items.Modify(Item, p => p with { Name = value }));
    }

    [RelayCommand]
    private async Task Remove()
    {
        await _model.Run<IMutableToDoModelShard>((shard, _) => shard.Items.Remove(Item));
    }
}
