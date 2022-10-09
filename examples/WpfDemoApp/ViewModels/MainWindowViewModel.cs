using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Subscription;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfDemoApp.Model;
using WpfDemoApp.Model.Entities;
using WpfDemoApp.ViewModels;

namespace WpfDemoApp;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly IDisposable _subscription; // Dispose to unsubscribe
    private readonly UndoRedoDomainModel _model;

    [ObservableProperty]
    private string? _newItemName;

    public MainWindowViewModel(UndoRedoDomainModel model)
    {
        _model = model;

        Items = new ObservableCollection<ItemViewModel>();
        Logs = new ObservableCollection<string>();

        _subscription = _model.SubscribeTo<IToDoChangesFrame>(x => x.With(x => x.Items).By(OnItemChanged));
        _model.Changed += OnModelChanged; // unsubscribe
    }

    public ObservableCollection<ItemViewModel> Items { get; }

    public ObservableCollection<string> Logs { get; }

    [RelayCommand]
    private async Task Add()
    {
        if (NewItemName != null)
        {
            await _model.Run<IMutableToDoModelShard>(
                (shard, _) => shard.Items.Add(new() { Name = NewItemName }));

            NewItemName = null;
        }
    }

    [RelayCommand]
    private async Task Undo()
    {
        await _model.Undo();
    }

    [RelayCommand]
    private async Task Redo()
    {
        await _model.Redo();
    }

    private void OnItemChanged(Change<ICollectionChangeSet<ToDoItem, ToDoItemProperties>> change)
    {
        var (_, _, hunk) = change;

        foreach (var c in hunk.Where(x => x.Action == CollectionAction.Add))
        {
            Items.Add(new ItemViewModel(c.Entity, c.NewData!, _model));
        }

        foreach (var c in hunk.Where(x => x.Action == CollectionAction.Remove))
        {
            Items.Remove(Items.Single(x => x.Item == c.Entity));
        }

        foreach (var c in hunk.Where(x => x.Action == CollectionAction.Modify))
        {
            Items.Single(x => x.Item == c.Entity).Name = c.NewData!.Name;
        }
    }

    private void OnModelChanged(object? sender, EventArgs e)
    {
        Logs.Clear();

        foreach (var change in _model.UndoStack)
        {
            if (change.TryGetFrame<IToDoChangesFrame>(out var frame) && frame.HasChanges())
            {
                foreach (var log in FormatChanges(frame))
                {
                    Logs.Add(log);
                }
            }
        }
    }

    private IEnumerable<string> FormatChanges(IToDoChangesFrame toDoChanges)
    {
        foreach (var change in toDoChanges.Items)
        {
            var builder = new StringBuilder();
            builder.Append(change.Action);
            builder.AppendLine();
            builder.Append(change.Entity);

            if (change.Action == CollectionAction.Add || change.Action == CollectionAction.Modify)
            {
                builder.AppendLine();
                builder.Append(change.NewData);
            }

            if (change.Action == CollectionAction.Remove || change.Action == CollectionAction.Modify)
            {
                builder.AppendLine();
                builder.Append(change.OldData);
            }

            yield return builder.ToString();
        }
    }
}
