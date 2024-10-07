using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MockupImageProccessing.Builders;

namespace MockupImageProccessing.Services;

internal class WindowService : IWindowService
{
    private bool _exceptionDialogOpen;
    private Ioc _container;

    public WindowService()
    {
        _container = Ioc.Default;
    }

    public Window ShowWindow<T>(out T viewModel)
    {
        viewModel = _container.GetRequiredService<T>();
        if (viewModel == null)
            throw new Exception(
                $"Failed to show window for VM of type {typeof(T).Name}, could not create instance.");

        return ShowWindow(viewModel);
    }

    public Window ShowWindow(object viewModel)
    {
        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new Exception($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new Exception($"Type {name} is not a window.");

        Window window = (Window)Activator.CreateInstance(type)!;
        window.DataContext = viewModel;
        window.Show();

        return window;
    }

    public Window ShowWindow(object viewModel, int screen)
    {
        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new Exception($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new Exception($"Type {name} is not a window.");

        Window window = (Window)Activator.CreateInstance(type)!;
        window.DataContext = viewModel;

        var monitor = window.Screens.All.Count <= screen ? window.Screens.All.First() : window.Screens.All[screen];
        if (monitor != null)
        {
            window.Position = monitor.WorkingArea.TopLeft;
            window.Show();
        }

        return window;
    }

    public Window ShowWindow(object viewModel, PixelPoint startupLocation, Size windowSize)
    {
        string name = viewModel.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = viewModel.GetType().Assembly.GetType(name);

        if (type == null)
            throw new Exception($"Failed to find a window named {name}.");

        if (!type.IsAssignableTo(typeof(Window)))
            throw new Exception($"Type {name} is not a window.");

        Window window = (Window)Activator.CreateInstance(type)!;
        window.DataContext = viewModel;
        window.Position = startupLocation;
        window.Width = windowSize.Width;
        window.Height = windowSize.Height;
        window.Show();


        return window;
    }

    public OpenFileDialogBuilder CreateOpenFileDialog()
    {
        Window? currentWindow = GetCurrentWindow();
        if (currentWindow == null)
            throw new Exception("Can't show an open file dialog without any windows being shown.");
        return new OpenFileDialogBuilder(currentWindow);
    }

    public SaveFileDialogBuilder CreateSaveFileDialog()
    {
        Window? currentWindow = GetCurrentWindow();
        if (currentWindow == null)
            throw new Exception("Can't show a save file dialog without any windows being shown.");
        return new SaveFileDialogBuilder(currentWindow);
    }

    public Window? GetCurrentWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime classic)
            throw new Exception(
                "Find an open window when application lifetime is not IClassicDesktopStyleApplicationLifetime.");

        Window? parent = classic.Windows.FirstOrDefault(w => w.IsActive && w.ShowInTaskbar) ?? classic.MainWindow;
        return parent;
    }
}