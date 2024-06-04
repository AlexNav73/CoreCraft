using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreCraft;
using CoreCraft.Storage.Sqlite;
using Microsoft.Win32;
using WpfDemoApp.Model.Entities;
using WpfDemoApp.ViewModels.Pages;

namespace WpfDemoApp;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly UndoRedoDomainModel _model;
    private readonly Func<string, ISqliteStorage> _storageFactory;

    private readonly ObservableObject _homePage;

    [ObservableProperty]
    private ObservableObject? _page;

    public MainWindowViewModel(UndoRedoDomainModel model, Func<string, ISqliteStorage> storageFactory)
    {
        _model = model;
        _storageFactory = storageFactory;

        _homePage = new HomePageViewModel(model);
        _page = _homePage;
    }

    [RelayCommand]
    private async Task Undo()
    {
        await _model.History.Undo();
    }

    [RelayCommand]
    private async Task Redo()
    {
        await _model.History.Redo();
    }

    [RelayCommand]
    private async Task Save()
    {
        var saveFileDialog = new SaveFileDialog();

        if (saveFileDialog.ShowDialog() == true)
        {
            if (File.Exists(saveFileDialog.FileName))
            {
                await _model.History.Update(_storageFactory(saveFileDialog.FileName));
            }
            else
            {
                await _model.Save(_storageFactory(saveFileDialog.FileName));
            }
        }
    }

    [RelayCommand]
    private async Task Open()
    {
        var openFileDialog = new OpenFileDialog();

        if (openFileDialog.ShowDialog() == true)
        {
            await _model.Load(_storageFactory(openFileDialog.FileName));
        }
    }

    [RelayCommand]
    private void Select(object parameter)
    {
        Page = new ItemsPageViewModel(_model, (ToDoList)parameter);
    }

    [RelayCommand]
    private void GoHome()
    {
        Page = _homePage;
    }
}
