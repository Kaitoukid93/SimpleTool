using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using MockupImageProccessing.ViewModels;

namespace MockupImageProccessing.Views;

public partial class MainWindow : Window
{
    [DllImport("Kernel32")]
    public static extern void AllocConsole();

    [DllImport("Kernel32", SetLastError = true)]
    public static extern void FreeConsole();

    public MainWindow()
    {
        InitializeComponent();
        var nonContentAreaVm = Ioc.Default.GetRequiredService<NonClientAreaContentViewModel>();
        nonContentAreaVm.ToggleConsoleEvent += OnConsoleToggled;
    }

    private void OnConsoleToggled(bool isOpen)
    {
        if (isOpen)
        {
            AllocConsole();
            Console.WriteLine("Welcome to Simple tool");
        }
        else
        {
            FreeConsole();
        }
    }
}