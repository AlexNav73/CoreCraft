using Autofac;
using MahApps.Metro.Controls;
using CoreCraft.Engine;
using CoreCraft.Engine.Core;
using CoreCraft.Engine.Persistence;
using CoreCraft.Engine.Scheduling;
using CoreCraft.Storage.Sqlite;
using System.Collections.Generic;
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

        var builder = new ContainerBuilder();

        builder.RegisterType<ToDoModelShardStorage>().As<IModelShardStorage>();
        builder.RegisterType<SqliteStorage>().As<IStorage>();
        builder.RegisterType<ToDoModelShard>().As<IModelShard>();
        builder.Register(c => new UndoRedoDomainModel(
            c.Resolve<IEnumerable<IModelShard>>(),
            new AsyncScheduler(),
            c.Resolve<IStorage>()));
        builder.RegisterType<MainWindowViewModel>().AsSelf();

        var container = builder.Build();

        DataContext = container.Resolve<MainWindowViewModel>();
    }
}
