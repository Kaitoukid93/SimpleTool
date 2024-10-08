using System;
using System.Reflection.Emit;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace MockupImageProccessing.ViewModels;

public class NonClientAreaContentViewModel : ViewModelBase
{
    public event Action<bool> ToggleConsoleEvent;
    public NonClientAreaContentViewModel()
    {
        ToggleConsoleCommand = new RelayCommand(ToggleConsole);
    }

    private void ToggleConsole()
    {
        ToggleConsoleEvent?.Invoke(EnableConsole);
    }

    public ICommand ToggleConsoleCommand { get; }
    public bool EnableConsole { get; set; }
}