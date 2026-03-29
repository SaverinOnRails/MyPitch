using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MyPitch.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

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
}