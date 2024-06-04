using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreCraft;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Extensions;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ViewModels;

internal partial class ToDoItemListViewModel : ObservableObject, IHasEntity<ToDoList>
{
    private readonly IReadOnlyList<IDisposable> _subscriptions; // Dispose to unsubscribe
    
    private readonly UndoRedoDomainModel _model;

    [ObservableProperty]
    public string? _name;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Completed))]
    public int _total;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Completed))]
    public int _checked;

    public ToDoItemListViewModel(UndoRedoDomainModel model, ToDoList entity, ToDoListProperties properties)
    {
        _model = model;

        Entity = entity;
        Name = properties.Name;

        _subscriptions =
        [
            model.For<IToDoChangesFrame>()
                .With(x => x.Items)
                .Bind(OnItemsChanged),

            model.For<IToDoChangesFrame>()
                .With(x => x.ListToItems)
                .Subscribe(OnListToItemsChanged)
        ];
    }

    public ToDoList Entity { get; }

    public double Completed => Total != 0 ? ((double)Checked) / Total * 100.0 : 0;

    [RelayCommand]
    private async Task Remove()
    {
        await _model.Run<IMutableToDoModelShard>((shard, _) =>
        {
            shard.ListToItems.Remove(Entity);
            shard.Lists.Remove(Entity);
        });
    }

    private void OnItemsChanged(Change<CollectionChangeGroups<ToDoItem, ToDoItemProperties>> changes)
    {
        UpdateCounters(changes.NewModel);
    }

    private void OnListToItemsChanged(Change<IRelationChangeSet<ToDoList, ToDoItem>> changes)
    {
        if (changes.Hunk.Any(x => x.Parent == Entity))
        {
            UpdateCounters(changes.NewModel);
        }
    }

    private void UpdateCounters(IModel model)
    {
        var shard = model.Shard<IToDoModelShard>();

        Checked = shard.ListToItems.Children(Entity).Count(x => shard.Items.Get(x).IsChecked);
        Total = shard.ListToItems.Children(Entity).Count();
    }
}
