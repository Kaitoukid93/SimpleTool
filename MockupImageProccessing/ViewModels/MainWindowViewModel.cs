using System.Collections.Generic;
using System.Linq;

namespace MockupImageProccessing.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ImageSeparationViewModel _imageSeparationViewModel;
    
    public MainWindowViewModel(ImageSeparationViewModel imageSeparationViewModel, AboutViewModel aboutViewModel,NonClientAreaContentViewModel nonClientAreaContentViewModel)
    {
        NonClientAreaContent = nonClientAreaContentViewModel;
        _imageSeparationViewModel = imageSeparationViewModel;
        SideMenuItems = new List<SideMenuItemViewModel>()
        {
            new SideMenuItemViewModel()
            {
                Name = "MIP",
                Icon = "mip",
                Description = "Mockup image processing",
                Page = _imageSeparationViewModel
            },
            new SideMenuItemViewModel()
            {
            Name = "About",
            Icon = "about",
            Description = "About the author",
            Page = aboutViewModel
        }
        };
        SelectedView = SideMenuItems.First();
    }

   

    public List<SideMenuItemViewModel> SideMenuItems { get; set; }
    private ViewModelBase _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public NonClientAreaContentViewModel NonClientAreaContent { get; }
    private SideMenuItemViewModel _selectedView;

    public SideMenuItemViewModel SelectedView
    {
        get=> _selectedView;
        set
        {
            _selectedView = value;
            Navigate();
            OnPropertyChanged();
        }
    }

    private void Navigate()
    {
        CurrentViewModel = SelectedView.Page;
    }
}