using System;
using System.Collections.Generic;
using System.Windows;
using Autofac;
using CoreCraft;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Storage.Sqlite;
using CoreCraft.Storage.Sqlite.Migrations;
using WpfDemoApp.Model;

namespace WpfDemoApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
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
