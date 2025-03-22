using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using MockupImageProccessing.Extension;
using MockupImageProccessing.Services;
using MockupImageProccessing.Services.ImageProcessingService;
using Size = Avalonia.Size;

namespace MockupImageProccessing.ViewModels;

public class ImageSeparationViewModel : ViewModelBase
{
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


    public ImageSeparationViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        Icon = "ImageSeparation";
        Name = "Image background remover";
        Description = "Mask out image using predefined geometry path";
        OpenImageBrowserCommand = new AsyncRelayCommand(BrowseImage);
        ProcessImageCommand = new AsyncRelayCommand(ProcessImage);
        StopCommand = new RelayCommand(Stop);
        OpenSelectOutputDirectoryCommand = new AsyncRelayCommand(SelectOutputFolder);
        Prefix = "MIP";
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        OutputDirectory = Path.Combine(path, "MIPOutput");
        Directory.CreateDirectory(OutputDirectory);
        FileFormat = AvailableFormats[0];
        ThresholdFactor = 40;
    }

    private bool _shouldBeRunning = false;

    private void Stop()
    {
        _shouldBeRunning = false;
    }


    private async Task ProcessImage()
    {
        ErrorImages = new List<string>();
        ShowErrorMessage = false;
        IsRendering = true;
        _shouldBeRunning = true;
        var timer = new Stopwatch();
        timer.Start();
        var imageProcessor = new ImageBackgroundSeparatorService();
        imageProcessor.ErrorOccured += OnErrorOccured;
        int count = 0;
        if (!Directory.Exists(OutputDirectory))
            Directory.CreateDirectory(OutputDirectory);
        var size = new Size(ImageSizeWidth, ImageSizeHeight);
        foreach (var image in _selectedImages)
        {
            if (!_shouldBeRunning)
                break;
            CurrentProgress = (int)((count * 100d / (double)_selectedImages.Count));
            TimeElapsed = count + "/" + _selectedImages.Count + " " + "exported";
            var result = await Task.Run(() => imageProcessor.ProcessImage(image, ShowContour, ShowRectangle,
                _right2BotRatio, _left2TopRatio, _whRatio, size,ThresholdFactor));
            if (result == null)
            {
                Console.WriteLine("Problem processing image, could be image with black background " + image);
                continue;
            }

            DisplayImage = result.ConvertToAvaloniaBitmap();
            var fileName = Prefix + "-" + count++ + ".jpg";
            if (UseOriginalName)
            {
                fileName = Path.GetFileNameWithoutExtension(image) + FileFormat;
            }
            
            var filePath = Path.Combine(OutputDirectory, fileName);
            DisplayImage.Save(filePath);
            Thread.Sleep(Interval);
        }

        IsRendering = false;

        //B: Run stuff you want timed
        timer.Stop();
        TimeSpan timeTaken = timer.Elapsed;
        TimeElapsed = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");

        if (ErrorImages.Count > 0)
        {
            ErrorMessage = ErrorImages.Count + " Problems found!!!";
            ShowErrorMessage = true;
            System.IO.File.WriteAllLines(Path.Combine(OutputDirectory,"Error.txt"), ErrorImages);
        }
        if (OpenOutputDirectory)
        {
            OpenDierectory(OutputDirectory);
        }

        // DisplayImage = null;
    }

    private List<string> _errorImages = new List<string>();

    public List<String> ErrorImages
    {
        get => _errorImages;
        set
        {
            _errorImages = value;
            OnPropertyChanged();
        }
    }
    private string _errorMessage ;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }
    private void OnErrorOccured(string obj)
    {
        ErrorImages.Add(obj);
    }

    private bool _showErrorMessage;

    public bool ShowErrorMessage
    {
        get => _showErrorMessage;
        set
        {
            _showErrorMessage = value;
            OnPropertyChanged();
        }
    }
    private void OpenDierectory(string directory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            Arguments = directory,
            FileName = "explorer.exe"
        };

        Process.Start(startInfo);
    }

    private int _interval = 10;

    public int Interval
    {
        get => _interval;
        set
        {
            _interval = value;
            OnPropertyChanged();
        }
    }

    private async Task BrowseImage()
    {
        _selectedImages?.Clear();
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("bmp").WithExtension("jpg").WithExtension("png").WithExtension("webp").WithName("image file"))
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

    private bool _showContour;

    public bool ShowContour
    {
        get => _showContour;
        set
        {
            _showContour = value;
            OnPropertyChanged();
        }
    }

    private bool _showRectangle;

    public bool ShowRectangle
    {
        get => _showRectangle;
        set
        {
            _showRectangle = value;
            OnPropertyChanged();
        }
    }

    private double _right2BotRatio = 395d / 540d;
    private double _left2TopRatio = 270d / 314d;
    private double _whRatio = 512d / 672d;

    public double Right2BotRatio
    {
        get => _right2BotRatio;
        set
        {
            _right2BotRatio = value;
            OnPropertyChanged();
        }
    }

    public double Left2TopRatio
    {
        get => _left2TopRatio;
        set
        {
            _left2TopRatio = value;
            OnPropertyChanged();
        }
    }

    public double WHRatio
    {
        get => _whRatio;
        set
        {
            _whRatio = value;
            OnPropertyChanged();
        }
    }

    private int _imageSizeWidth = 1000;

    public int ImageSizeWidth
    {
        get => _imageSizeWidth;
        set
        {
            _imageSizeWidth = value;
            OnPropertyChanged();
        }
    }

    private int _imageSizeHeight = 1000;

    public int ImageSizeHeight
    {
        get => _imageSizeHeight;
        set
        {
            _imageSizeHeight = value;
            OnPropertyChanged();
        }
    }

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

    private bool _useOriginalName = true;

    public bool UseOriginalName
    {
        get => _useOriginalName;
        set
        {
            _useOriginalName = value;
            OnPropertyChanged();
        }
    }

    private string _timeElapsed = "Time taken show here";

    public string TimeElapsed
    {
        get => _timeElapsed;
        set
        {
            _timeElapsed = value;
            OnPropertyChanged();
        }
    }

    private int _currentProgress;

    public int CurrentProgress
    {
        get => _currentProgress;
        set
        {
            _currentProgress = value;
            OnPropertyChanged();
        }
    }
    private int _thresholdFactor;

    public int ThresholdFactor
    {
        get => _thresholdFactor;
        set
        {
            _thresholdFactor = value;
            OnPropertyChanged();
        }
    }
    private string _fileFormat = ".jpg";

    public string FileFormat
    {
        get => _fileFormat;
        set
        {
            _fileFormat = value;
            OnPropertyChanged();
        }
    }

    public ICommand StopCommand { get; set; }
    public List<string> AvailableFormats => [".jpg", ".png", ".webp"];
}