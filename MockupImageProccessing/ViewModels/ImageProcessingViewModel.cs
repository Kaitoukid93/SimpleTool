using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using MockupImageProccessing.Extension;
using MockupImageProccessing.Services;
using MockupImageProccessing.Services.ImageProcessingService;

namespace MockupImageProccessing.ViewModels;

public class ImageProcessingViewModel : ViewModelBase
{
    public ImageProcessingViewModel()
    {
    }

    private IWindowService _windowService;
    private string _prefix;

    public string Prefix
    {
        get => _prefix;
        set
        {
            _prefix = value;
            OnPropertyChanged();
        }
    }

    private string _outputDirectory;

    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            _outputDirectory = value;
            OnPropertyChanged(nameof(CanRender));
            OnPropertyChanged();
        }
    }

    private bool _openOutputDirectory;

    public bool OpenOutputDirectory
    {
        get => _openOutputDirectory;
        set
        {
            _openOutputDirectory = value;
            OnPropertyChanged();
        }
    }


    public ImageProcessingViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenImageBrowserCommand = new AsyncRelayCommand(BrowseImage);
        ProcessImageCommand = new AsyncRelayCommand(ProcessImage);
        OpenSelectOutputDirectoryCommand = new AsyncRelayCommand(SelectOutputFolder);
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        OutputDirectory = Path.Combine(path, "MIPOutput");
        Directory.CreateDirectory(OutputDirectory);
    }


    private async Task ProcessImage()
    {
        var imageProcessor = new EmguCVExampleService();
        var result = imageProcessor.ProcessImage(_selectedImages.First());
        DisplayImage = result.ConvertToAvaloniaBitmap();
    }

    #region process image loop recomendation
    // The loop that in charge of receiving camera data has to call  imageProcessor.ProcessImage(image)
    // every time an image has been processed, Invoke an event to this viewmodel (OnNewImageProcessed(ProcessedImage))
    // This viewmodel will call a single method :
    // ConvertToAvaloniaBitmap(ProcessedImage,DisplayImage)
    // This method is using same DisplayImage data so no new Bitmap is created
    // if you dont care about perfomance hit, just use the method above
    #endregion


    private void OpenDirectory(string directory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            Arguments = directory,
            FileName = "explorer.exe"
        };

        Process.Start(startInfo);
    }

    private async Task BrowseImage()
    {
        _selectedImages?.Clear();
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("jpg").WithExtension("png").WithExtension("webp").WithName("image file"))
            .WithAllowMultiple()
            .ShowAsync();
        if (result == null)
        {
            ImagesPath = "Click browse to select image";
            return;
        }

        _selectedImages = result.ToList();
        ImagesPath = result.Length + " images selected";
        OnPropertyChanged(nameof(ShowNoImageText));
        OnPropertyChanged(nameof(CanRender));
    }

    private async Task SelectOutputFolder()
    {
        string result = await _windowService.CreateSaveFileDialog()
            .WithInitialFileName("Log.txt")
            .ShowAsync();
        if (result == null)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            OutputDirectory = Path.Combine(path, "MIPOutput");
            Directory.CreateDirectory(OutputDirectory);
            return;
        }

        OutputDirectory = Path.GetDirectoryName(result);
        //ProcessImageCommand.NotifyCanExecuteChanged();
    }

    private List<string> _selectedImages;
    public string Icon { get; set; }
    public string Name { get; set; }
    private Avalonia.Media.Imaging.Bitmap _displayImage;
    public bool ShowNoImageText => _selectedImages == null || _selectedImages.Count == 0;
    public bool CanRender => _selectedImages != null && OutputDirectory != null;

    public Avalonia.Media.Imaging.Bitmap DisplayImage
    {
        get => _displayImage;
        set
        {
            _displayImage = value;
            OnPropertyChanged();
        }
    }

    public string Description { get; }
    private string _imagesPath;

    public string ImagesPath
    {
        get => _imagesPath;
        set
        {
            _imagesPath = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenImageBrowserCommand { get; }
    public ICommand OpenSelectOutputDirectoryCommand { get; }
    public AsyncRelayCommand ProcessImageCommand { get; }


    private bool _isRendering;

    public bool IsRendering
    {
        get => _isRendering;
        set
        {
            _isRendering = value;
            OnPropertyChanged();
        }
    }
}