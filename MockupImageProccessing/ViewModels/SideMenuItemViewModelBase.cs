namespace MockupImageProccessing.ViewModels;

public class SideMenuItemViewModelBase : ViewModelBase
{
    public string Icon { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ViewModelBase Page { get; set; }
}