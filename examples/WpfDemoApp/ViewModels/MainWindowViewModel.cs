using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreCraft;
using CoreCraft.ChangesTracking;
using CoreCraft.Subscription;
using Microsoft.Win32;
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

        _subscription = _model
            .For<IToDoChangesFrame>()
            .With(x => x.Items)
            .Bind(Observer.Create<BindingChanges<ToDoItem, ToDoItemProperties>>(OnNext));
        _model.Changed += OnModelChanged; // unsubscribe
    }

    public ObservableCollection<ItemViewModel> Items { get; }

    public ObservableCollection<string> Logs { get; }

    private void OnNext(BindingChanges<ToDoItem, ToDoItemProperties> changes)
    {
        foreach (var c in changes.Added)
        {
            var viewModel = new ItemViewModel(c.Entity, c.NewData!, _model);

            _model.For<IToDoChangesFrame>().With(x => x.Items).Bind(c.Entity, viewModel);

            Items.Add(viewModel);
        }

        foreach (var c in changes.Removed)
        {
            Items.Remove(Items.Single(x => x.Entity == c.Entity));
        }
    }

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

    [RelayCommand]
    private async Task Save()
    {
        var saveFileDialog = new SaveFileDialog();

        if (saveFileDialog.ShowDialog() == true)
        {
            await _model.Save(saveFileDialog.FileName);
        }
    }

    [RelayCommand]
    private async Task Open()
    {
        var openFileDialog = new OpenFileDialog();

        if (openFileDialog.ShowDialog() == true)
        {
            await _model.Load(openFileDialog.FileName);
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

            if (change is { Action: CollectionAction.Add or CollectionAction.Modify })
            {
                builder.AppendLine();
                builder.Append(change.NewData);
            }

            if (change is { Action: CollectionAction.Remove or CollectionAction.Modify })
            {
                builder.AppendLine();
                builder.Append(change.OldData);
            }

            yield return builder.ToString();
        }
    }
}
