using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MyPitch.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyPitch.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        //TODO: Layout hack, find a good fix
    }


    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var insetsmanager = TopLevel.GetTopLevel(this)?.InsetsManager;
        if (insetsmanager is not null && System.OperatingSystem.IsAndroid())
        {
            insetsmanager.DisplayEdgeToEdgePreference = true;
        }
        if (System.OperatingSystem.IsWindows())
        {
            // Background = Brushes.Transparent; //will fallback to mica
        }
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if(change.Property == BoundsProperty)
        {
            var datacontext = DataContext as MainViewModel;
            if (datacontext is null) return;
            if(Bounds.Width > 600)
            {
                datacontext.WideLayout = true;
            }
            else
            {
                datacontext.WideLayout = false;
            }
        }
    }
}