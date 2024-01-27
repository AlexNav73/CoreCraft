using Autofac;
using MahApps.Metro.Controls;
using CoreCraft;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Storage.Sqlite;
using System.Collections.Generic;
using WpfDemoApp.Model;
using System;
using CoreCraft.Storage.Sqlite.Migrations;

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

        builder.Register<Func<string, IStorage>>(c =>
        {
            var migrations = c.Resolve<IEnumerable<IMigration>>();

            return path => new SqliteStorage(path, migrations);
        });
        builder.RegisterType<ToDoModelShard>().As<IModelShard>();
        builder.Register(c => new UndoRedoDomainModel(
            c.Resolve<IEnumerable<IModelShard>>()));
        builder.RegisterType<MainWindowViewModel>().AsSelf();

        var container = builder.Build();

        DataContext = container.Resolve<MainWindowViewModel>();
    }
}
