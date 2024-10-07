using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MockupImageProccessing.Services;
using MockupImageProccessing.ViewModels;
using MockupImageProccessing.Views;

namespace MockupImageProccessing;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureIoc();
        var mainWindowVm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowVm,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureIoc()
    {
        var service = new ServiceCollection();
        service.AddSingleton<MainWindowViewModel>();
        service.AddSingleton<ImageSeparationViewModel>();
        service.AddSingleton<IWindowService, WindowService>();
        service.AddSingleton<AboutViewModel>();
        service.AddSingleton<NonClientAreaContentViewModel>();
        var serviceProvider = service.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
    }
}