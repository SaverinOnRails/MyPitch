using Avalonia;
using Avalonia.Controls;
using MyPitch.Controls;
using MyPitch.ViewModels;
namespace MyPitch.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    private bool _configuredLayout = false;
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var insetsmanager = TopLevel.GetTopLevel(this)?.InsetsManager;
        (DataContext as MainViewModel)._dialogHost = this.DialogHost; //this is the easiest way, sue me
        if (insetsmanager is not null && System.OperatingSystem.IsAndroid())
        {
            insetsmanager.DisplayEdgeToEdgePreference = true;
        }
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_configuredLayout)
        {
            if (change.Property == BoundsProperty)
            {
                var datacontext = DataContext as MainViewModel;
                if (datacontext is null) return;
                if (Bounds.Width > 600)
                {
                    MainPanel.Children.Add(new MainContent() { Layout = Layout.Wide });
                }
                else
                {
                    MainPanel.Children.Add(new MainContent() { Layout = Layout.Narrow });
                }
                _configuredLayout = true;
            }
        }

    }
}
