using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyPitch.Models;
using MyPitch.ViewModels;
using System.Text.Json;
namespace MyPitch.Controls;

public class DialogHost : Panel
{

    private Typeface _notoSansTypeface => CircleOfFifths.NotoSansTypeface;
    public DialogHost()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Background = new SolidColorBrush(Colors.Black, 0.7);
        IsVisible = false;
    }
    public void Show(IDialogContent? content, object? source)
    {
        IsVisible = true;
        //get the parent size to know best to size the dialog
        var parentWidth = this.FindAncestorOfType<UserControl>()?.Bounds.Width;
        bool wideLayout = parentWidth > 600;
        Panel contentBox = new()
        {
            Height = 600,
            Width = wideLayout ? 800 : parentWidth!.Value,
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
            BuildInteractiveStatsDialogContent(e, mainContent, source);
        }
        if (content is FolkDatabaseDialogContent)
        {
            BuildFolkDatabaseDialogContent(mainContent, source);
        }
    }

    private void BuildFolkDatabaseDialogContent(Panel mainContent, object? source)
    {
        Grid mgrid = new() { RowDefinitions = new("Auto,*") };
        mainContent.Children.Add(mgrid);

        var mfFiles = EmbeddedResources.GetMelodyFiles();
        var header = new TextBlock() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new(20, 0, 0, 0), FontSize = 30, Text = $"Folk Database({mfFiles.Count})" };
        mgrid.Children.Add(header);

        var scrollViewer = new ScrollViewer() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch };
        var bpanel = new WrapPanel() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch, ItemSpacing = 10, LineSpacing = 10, Margin = new(10) };
        scrollViewer.Content = bpanel;
        Grid.SetRow(scrollViewer, 1);
        mgrid.Children.Add(scrollViewer);

        //get json files
        foreach (var file in mfFiles)
        {
            var formattedName = file;
            const string prefix = "MyPitch.FolkDatabase.";
            if (formattedName.StartsWith(prefix))
            {
                formattedName = formattedName[prefix.Length..];
            }

            if (formattedName.EndsWith(".mf", StringComparison.OrdinalIgnoreCase))
            {
                formattedName = formattedName[..^3];
            }

            formattedName = formattedName.Replace("_", " ");
            var button = new Button() { Content = formattedName };
            button.Classes.Add("Ghost");
            button.Click += (_, _) =>
            {
                if (source is MainViewModel vm)
                {
                    vm.Game.SetFolkModeMelodyFile(file);
                }
                Hide();
            };
            bpanel.Children.Add(button);
        }
    }

    private void BuildInteractiveStatsDialogContent(InteractiveModeStatsDialogContent e, Panel contentBox, object? source)
    {
        bool narrowLayout = this.FindAncestorOfType<UserControl>()?.Bounds.Width <= 600;
        Grid mGrid = new() { RowDefinitions = new(narrowLayout ? "Auto,Auto,*" : "Auto,*") };
        var tPanel = new Panel();
        var circularProgress = new CircularProgress() { Height = 100, Width = 200 };
        tPanel.Children.Add(
            circularProgress
        );
        Button drillButton = new Button() { Content = "Play Weaknesses", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Height = 40, Margin = new(20), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom, Background = new SolidColorBrush(Color.Parse("#FFEDCE")) };
        drillButton.Classes.Add("Primary");

        static void ApplySelections<T>(
            IList<MultiSelectableItem<T>> items,
            List<string> interests)
        {
            foreach (var item in items)
                item.IsSelected = interests.Contains(item.Label);
        }
        drillButton.Click += (s, _) =>
            {
                if (source is MainViewModel vm)
                {
                    var interests = e.Stats.Stats
                        .Where(p => p.Value.TimesIncorrect > 0)
                        .Select(p => p.Key)
                        .Concat(e.Stats.Stats.SelectMany(p => p.Value.MistakenFor))
                        .Distinct()
                        .ToList();
                    if (e.Stats.GameMode == GameMode.Interactive)
                        ApplySelections(vm.Degrees, interests);
                    else
                        ApplySelections(vm.ChordQualities, interests);
                    Hide();
                }
            };

        var parentWidth = this.FindAncestorOfType<UserControl>()?.Bounds.Width;
        if (!narrowLayout)
        {
            tPanel.Children.Add(drillButton);
        }
        else
        {
            Grid.SetRow(drillButton, 1);
            mGrid.Children.Add(drillButton);
        }
        circularProgress.Progress = e.Stats.Accuracy;
        mGrid.Children.Add(tPanel);
        Panel dGridPanel = new() { Margin = new(10) };
        Grid.SetRow(dGridPanel, narrowLayout ? 2 : 1);
        contentBox.Children.Add(mGrid);
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        };
        var dataModel = new List<InteractiveStatsTableViewModel>();
        foreach (var p in e.Stats.Stats)
        {
            dataModel.Add(new()
            {
                Data = p.Key,
                TimesCorrect = p.Value.TimesCorrect,
                TimesIncorrect = p.Value.TimesIncorrect,
                Familiarity = p.Value.Familiarity.ToString("F2"),
                AvgResponseTime = p.Value.AverageResponseTime.TotalSeconds.ToString("F1") + " secs",
                MistakenFor = p.Value.MistakenFor.Count > 0 ? String.Join(" ,", p.Value.MistakenFor) : " None"
            });
        }
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = e.Stats.GameMode == GameMode.Interactive ? "Degree" : "Chords",
            Binding = new Binding("Data"),
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
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Familiarity",
            Binding = new Binding("Familiarity"),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
        dataGrid.LoadingRow += (s, e) =>
        {
            if (e.Row.DataContext is null) return;
            var dataModel = (InteractiveStatsTableViewModel)e.Row.DataContext;
            if (dataModel?.TimesIncorrect > 0)
            {
                e.Row.Background = new SolidColorBrush(Colors.LightCoral, 0.4);
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
    public required string Data { get; set; }
    public int TimesCorrect { get; set; }
    public int TimesIncorrect { get; set; }
    public required string Familiarity { get; set; }
    public required string AvgResponseTime { get; set; }
    public string MistakenFor { get; set; } = "None";

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(InteractiveStatsTableViewModel))]
    private string dummyField = ""; //preserve this class for Native AOT
}
public interface IDialogContent;
public class InteractiveModeStatsDialogContent : IDialogContent
{
    public required InteractiveModeStats Stats;
}
public class FolkDatabaseDialogContent : IDialogContent { }
public delegate void DialogRequestedEventHandler(DialogRequestedEventArgs e);
public class DialogRequestedEventArgs
{
    public IDialogContent Content;
    public DialogRequestedEventArgs(IDialogContent content)
    {
        Content = content;
    }
}
