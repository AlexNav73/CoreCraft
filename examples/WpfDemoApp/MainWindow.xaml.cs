using MahApps.Metro.Controls;
using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Storage.Sqlite;
using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;
using System;
using WpfDemoApp.Model;

namespace WpfDemoApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var modelShardStorages = new IModelShardStorage[]
        {
            new ToDoModelShardStorage()
        };
        var storage = new SqliteStorage(Array.Empty<IMigration>(), modelShardStorages);
        var modelShards = new IModelShard[]
        {
            new ToDoModelShard()
        };
        var model = new UndoRedoDomainModel(modelShards, new AsyncScheduler(), storage);

        DataContext = new MainWindowViewModel(model);
    }
}
