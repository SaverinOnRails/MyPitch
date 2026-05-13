using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
    public void Show(IDialogContent? content = null)
    {
        IsVisible = true;
        //get the parent size to know best to size the dialog
        var parentWidth = this.FindAncestorOfType<UserControl>()?.Bounds.Width;
        bool wideLayout = parentWidth > 600;
        Panel contentBox = new()
        {
            Height = 500,
            Width = wideLayout ? 700 : parentWidth!.Value,
            Background = new SolidColorBrush(Color.Parse("#080616"))
        };
        Grid mGrid = new() { RowDefinitions = new("Auto,*") };
        var header = new Panel();
        var cancelButton = new Button() { Content = new TextBlock() { Text = "×", FontSize = 35 }, Width = 60, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
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
        Debug.WriteLine(e.Stats.Accuracy);
        var circularProgress = new CircularProgress() { Height = 100, Width = 200 };
        tPanel.Children.Add(
            circularProgress
        );
        circularProgress.Progress = e.Stats.Accuracy;
        mGrid.Children.Add(tPanel);
        Panel dGridPanel = new() { Margin = new(10) };
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
                AvgResponseTime = p.Value.AverageResponseTime.TotalSeconds.ToString("F1") + " secs",
                MistakenFor = p.Value.MistakenFor.Count > 0 ? String.Join(',', p.Value.MistakenFor) : " None"
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
            Header = "Avg. Response Time",
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
        dataGrid.LoadingRow += (s, e) =>
        {
            if(e.Row.DataContext is null) return;
            var dataModel = (InteractiveStatsTableViewModel)e.Row.DataContext;
            if (dataModel?.TimesIncorrect > 0)
            {
                e.Row.Background = new SolidColorBrush(Colors.LightCoral, 0.6);
            }
            else
            {
                e.Row.Background = Brushes.Transparent;
            }
        };
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
