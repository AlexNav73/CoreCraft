using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreCraft;
using CoreCraft.Core;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Extensions;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ViewModels.Pages;

internal sealed partial class ItemsPageViewModel : ObservableObject, IHasEntity<ToDoList>
{
    private readonly IDisposable _subscription; // Dispose to unsubscribe
    private readonly UndoRedoDomainModel _model;

    private readonly ObservableCollection<ToDoItemViewModel> _items;

    [ObservableProperty]
    private string? _newItemName;

    public ItemsPageViewModel(UndoRedoDomainModel model, ToDoList list)
    {
        _model = model;

        var shard = model.Shard<IToDoModelShard>();
        var items = shard.ListToItems
            .Children(list)
            .Select(x => new ToDoItemViewModel(x, shard.Items.Get(x), model));

        _items = new(items);

        Entity = list;
        Items = CollectionViewSource.GetDefaultView(_items);

        Items.SortDescriptions.Add(new SortDescription(nameof(ToDoItemViewModel.IsChecked), ListSortDirection.Ascending));

        _subscription = _model
            .For<IToDoChangesFrame>()
            .With(x => x.Items)
            .Bind(OnItemsChanged);
    }

    public ToDoList Entity { get; }

    public ICollectionView Items { get; }

    [RelayCommand]
    private async Task Add()
    {
        if (NewItemName != null)
        {
            await _model.Run<IMutableToDoModelShard>(
                (shard, _) => 
                {
                    var item = shard.Items.Add(new() { Name = NewItemName });
                    shard.ListToItems.Add(Entity, item);
                });

            NewItemName = null;
        }
    }

    private void OnItemsChanged(Change<CollectionChangeGroups<ToDoItem, ToDoItemProperties>> changes)
    {
        foreach (var change in changes.Hunk.Removed)
        {
            var viewModel = _items.Single(x => x.Entity == change.Entity);
            _items.Remove(viewModel);
        }

        foreach (var change in changes.Hunk.Added)
        {
            _items.Add(new ToDoItemViewModel(change.Entity, change.NewData!, _model));
        }

        Items.Refresh();
    }
}
