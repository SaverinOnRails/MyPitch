using System;

namespace MyPitch.Views;

public partial class MainWindow : ShadUI.Window
{
    public MainWindow()
    {
        InitializeComponent();
        //hack for tiling compositors
        if (System.OperatingSystem.IsLinux() && ShouldHideTitleBar())
        {
            IsTitleBarVisible = false;
        }
    }
    private bool ShouldHideTitleBar()
    {
        try
        {
            var gtk_csd = Environment.GetEnvironmentVariable("GTK_CSD");
            if (gtk_csd is not null)
            {
                if (gtk_csd == "1" || gtk_csd.ToLower() == "true") return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
