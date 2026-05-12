using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyPitch.Models;
namespace MyPitch.Controls;

public class DialogHost : Panel
{

    private Typeface _notoSansTypeface = new Typeface("avares://MyPitch/Assets/Fonts/#Noto Sans");
    public DialogHost()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Background = new SolidColorBrush(Colors.Black, 0.7);
        IsVisible = false;
        // Show();
    }

    public void Show(IDialogContent content)
    {
        IsVisible = true;
        //get the parent size to know best to size the dialog
        var parentWidth = this.FindAncestorOfType<UserControl>()?.Bounds.Width;
        bool wideLayout = parentWidth > 600;
        Panel contentBox = new()
        {
            Height = 500,
            Width = wideLayout ? 700 : 300,
            Background = new SolidColorBrush(Color.Parse("#080616"))
        };
        Grid mGrid = new() { RowDefinitions = new("Auto,*") };
        var header = new Panel();
        var cancelButton = new Button() { Content = new TextBlock() { Text = "×", FontSize = 20 }, Width = 60, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
        header.Children.Add(cancelButton);
        cancelButton.Click += (s, e) =>
        {
            Hide();
        };
        Panel mainContent = new();
        Grid.SetRow(header, 0);
        Grid.SetRow(mainContent, 1);
        mGrid.Children.Add(header);
        mGrid.Children.Add(mainContent);
        contentBox.Children.Add(mGrid);
        Children.Add(contentBox);

        if (content is InteractiveModeStatsDialogContent e)
        {
            BuildInteractiveStatsDialogContent(e, mainContent);
        }
    }

    private void BuildInteractiveStatsDialogContent(InteractiveModeStatsDialogContent e, Panel contentBox)
    {
        Grid mGrid = new() { RowDefinitions = new("Auto,*") };
        var tPanel = new Panel();
        tPanel.Children.Add(new TextBlock() { Text = $"Average response time: {e.Stats.AverageResponseTime.TotalSeconds.ToString("F1")}", TextAlignment = TextAlignment.Center, FontSize = 20 });
        mGrid.Children.Add(tPanel);

        Panel dGridPanel = new();
        Grid.SetRow(dGridPanel, 1);
        contentBox.Children.Add(mGrid);
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        };
        var dataModel = new List<InteractiveStatsTableViewModel>();
        foreach (var p in e.Stats.DegreeStats)
        {
            dataModel.Add(new()
            {
                Degree = p.Key,
                TimesCorrect = p.Value.TimesCorrect,
                TimesIncorrect = p.Value.TimesIncorrect,
                AvgResponseTime = p.Value.AverageResponseTime.TotalSeconds.ToString("F1"),
                MistakenFor = p.Value.MistakenFor.Count > 0 ? String.Join(',', p.Value.MistakenFor) : "None"
            });
        }
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Degree",
            Binding = new Binding("Degree"),
            IsReadOnly = true,
            FontFamily = _notoSansTypeface.FontFamily,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Times Correct",
            Binding = new Binding("TimesCorrect"),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Times Incorrect",
            Binding = new Binding("TimesIncorrect"),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Avg Response Time",
            Binding = new Binding("AvgResponseTime"),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Mistaken For",
            Binding = new Binding("MistakenFor"),
            FontFamily = _notoSansTypeface.FontFamily,
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        dataGrid.ItemsSource = dataModel;
        dGridPanel.Children.Add(dataGrid);
        mGrid.Children.Add(dGridPanel);
    }
    public void Hide()
    {
        IsVisible = false;
        Children.RemoveAll(Children);
    }
}

public class InteractiveStatsTableViewModel
{
    public required string Degree { get; set; }
    public int TimesCorrect { get; set; }
    public int TimesIncorrect { get; set; }
    public required string AvgResponseTime { get; set; }
    public string MistakenFor { get; set; } = "None";
}
public interface IDialogContent;
public class InteractiveModeStatsDialogContent : IDialogContent
{
    public required InteractiveModeStats Stats;
}
public delegate void DialogRequestedEventHandler(DialogRequestedEventArgs e);
public class DialogRequestedEventArgs
{
    public IDialogContent Content;
    public DialogRequestedEventArgs(IDialogContent content)
    {
        Content = content;
    }
}
