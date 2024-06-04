using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreCraft;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Extensions;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;

namespace WpfDemoApp.ViewModels.Pages;

internal sealed partial class HomePageViewModel : ObservableObject
{
    private readonly IDisposable _subscription; // Dispose to unsubscribe

    private readonly UndoRedoDomainModel _model;
    
    [ObservableProperty]
    private string? _newItemName;

    public HomePageViewModel(UndoRedoDomainModel model)
    {
        _model = model;

        Lists = new();

        _subscription = model.For<IToDoChangesFrame>()
            .With(x => x.Lists)
            .Bind(OnListsChanged);
    }

    public ObservableCollection<ToDoItemListViewModel> Lists { get; }

    [RelayCommand]
    private async Task Add()
    {
        if (NewItemName != null)
        {
            await _model.Run<IMutableToDoModelShard>(
                (shard, _) => shard.Lists.Add(new() { Name = NewItemName }));

            NewItemName = null;
        }
    }

    private void OnListsChanged(Change<CollectionChangeGroups<ToDoList, ToDoListProperties>> changes)
    {
        foreach (var removed in changes.Hunk.Removed)
        {
            var viewModel = Lists.Single(x => x.Entity == removed.Entity);
            Lists.Remove(viewModel);
        }

        foreach (var modified in changes.Hunk.Modified)
        {
            var viewModel = Lists.Single(x => x.Entity == modified.Entity);
            viewModel.Name = modified.NewData!.Name;
        }

        foreach (var added in changes.Hunk.Added)
        {
            Lists.Add(new ToDoItemListViewModel(_model, added.Entity, added.NewData!));
        }
    }
}
