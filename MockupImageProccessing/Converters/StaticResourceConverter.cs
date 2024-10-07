using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace MockupImageProccessing.Converters;

public class StaticResourceConverter : MarkupExtension, IValueConverter
{
    
    private Control _target;


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value.ToString() == string.Empty)
            return _target?.FindResource("nullIcon") ?? Application.Current.FindResource("nullIcon");
        var resourceKey = (string)value;

        return _target?.FindResource(resourceKey) ?? Application.Current.FindResource(resourceKey);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
        if (rootObjectProvider == null)
            return this;

        _target = rootObjectProvider.RootObject as Control;
        return this;
    }

}