using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using MyPitch.Models;

namespace MyPitch.Controls;

internal class ChordQualityInput : GameInputControl<ChordQuality>
{
    public ChordQualityInput()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
    }

    private void IncludedChordQualitiesChanged(object? sender, PropertyChangedEventArgs e)
    {
        Build();
    }
    protected override void DrawFunc() => Build();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Build();
    }

    void Build()
    {
        var selections = IncludedMultiSelectable.Where(p => p.IsSelected == true).ToList();
        int maxCols = Math.Min(selections.Count(), 3);

        Grid main = new() { RowDefinitions = new("Auto,*") };

        var uniformGrid = new UniformGrid
        {
            Columns = maxCols,
        };
        Grid.SetRow(uniformGrid, 1);
        for (int j = 0; j < selections.Count(); j++)
        {
            var chordQuality = selections[j];
            var buttonPanel = new PressableBorder() { Tag = chordQuality.Data };
            buttonPanel.Classes.Add("quality-input-button");
            buttonPanel.PointerPressed += buttonPanelPointerPressed;
            if (GameResponse is ChordQualityResponse r && r.Quality == chordQuality.Data)
            {
                if (AnswerState == AnswerState.Correct)
                    buttonPanel.Classes.Add("game-correct");
                else if (AnswerState == AnswerState.Incorrect)
                    buttonPanel.Classes.Add("game-incorrect");
            }
            var text = new TextBlock
            {
                Text = chordQuality.Label,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            buttonPanel.Child = text;
            uniformGrid.Children.Add(buttonPanel);
        }

        var topPanel = new Grid
        {
            Height = 50,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            ColumnDefinitions = new ColumnDefinitions("Auto,20,Auto")
        };
        Grid.SetRow(topPanel, 0);
        Button repeat = new() { Content = "Repeat", Width = 150, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        repeat.Click += repeatButtonClicked;
        repeat.Classes.Add("Destructive");

        var roundCounter = new TextBlock
        {
            FontSize = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        roundCounter.Bind(TextBlock.TextProperty, new Binding("Game.ChordQualityModeRounds")
        {
            StringFormat = "{0}/20"
        });

        Grid.SetColumn(repeat, 0);
        Grid.SetColumn(roundCounter, 2);
        topPanel.Children.Add(repeat);
        topPanel.Children.Add(roundCounter);
        main.Children.Add(topPanel);
        main.Children.Add(uniformGrid);
        Content = main;
    }

    private void repeatButtonClicked(object? sender, RoutedEventArgs e) => RepeatCommand.Execute(null);

    private void buttonPanelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is null || ((PressableBorder)sender).Tag is not ChordQuality qual) return;
        var chord = MusicTheory.BuildChord(Tonic, Tonic, qual);
        PlatformServiceProvider.AudioDriver.PlayChord(chord);
        if (e.Properties.IsLeftButtonPressed)
        {
            UserResponse = new ChordQualityResponse(qual);
        }
    }
}

public class PressableBorder : Border
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Add(":pressed");
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Remove(":pressed");
    }
}
