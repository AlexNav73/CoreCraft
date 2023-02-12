using Autofac;
using MahApps.Metro.Controls;
using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Storage.Sqlite;
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
