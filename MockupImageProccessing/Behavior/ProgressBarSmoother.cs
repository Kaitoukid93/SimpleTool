using System;
using Avalonia;
using Avalonia.Controls;

namespace MockupImageProccessing.Behavior;

public class ProgressBarSmoother
{
    public static AvaloniaProperty<double> ValueProperty =
        AvaloniaProperty.RegisterAttached<ProgressBarSmoother, ProgressBar, double>("Value");

    public static void SetValue(ProgressBar pb, double value) =>
        pb.SetValue(ValueProperty, value);

    static ProgressBarSmoother()
    {
        ValueProperty.Changed.Subscribe(ev => { ((ProgressBar)ev.Sender).Value = ev.NewValue.Value; });
    }
}