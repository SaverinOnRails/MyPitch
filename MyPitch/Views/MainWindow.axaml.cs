using System.Diagnostics;
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
            using var process = new Process();
            process.StartInfo.FileName = "gsettings";
            process.StartInfo.Arguments = "get org.gnome.desktop.wm.preferences button-layout";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            var error = process.StandardError.ReadToEnd();
            if (error is not null && error.Length > 0) return false;
            string output = process.StandardOutput.ReadToEnd();
            if (!output.Contains("maximise") && !output.Contains("minimize")) return true;
            process.WaitForExit();
            return false;
        }
        catch
        {
            return false;
        }
    }
}
