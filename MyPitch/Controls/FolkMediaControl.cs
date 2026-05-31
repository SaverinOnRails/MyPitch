using Avalonia.Controls;

namespace MyPitch.Controls;

public class FolkMediaControl : ContentControl
{
    public FolkMediaControl()
    {
        var stackPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Spacing = 10 };
        var progressbar = new Slider() { Width = 400 };
        var currentLocation = new TextBlock() { Text = "00:00", Margin = new(0, 15, 0, 0), FontSize = 20 };
        var totalDuration = new TextBlock() { Text = "00:00", FontSize = 20, Margin = new(0, 15, 0, 0) };
        stackPanel.Children.Add(currentLocation);
        stackPanel.Children.Add(progressbar);
        stackPanel.Children.Add(totalDuration);
        Content = stackPanel;
    }
}
