using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace MyPitch.Views;

public partial class MainContent : UserControl
{
    public MainContent()
    {
        InitializeComponent();

    }
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (Layout == Layout.Wide)
        {
            LayoutMain.ColumnDefinitions = new("*,Auto");
            // Grid.SetColumn(CircleHaloEffect, 0);
            Grid.SetColumn(CircleDisplay, 0);
            Grid.SetColumn(Card, 1);
            Card.MinWidth = 400;
        }
        else
        {
            LayoutMain.RowDefinitions = new("4*,*");
            // Grid.SetRow(CircleHaloEffect, 0);
            Grid.SetRow(CircleDisplay, 0);
            Grid.SetRow(Card, 1);
            Card.MinWidth = 250;
            (NarrowLayoutCardResizer as Button).Content = "☰";
        }
    }
    public Layout Layout { get; set; }

    private void NarrowLayoutCardResizer_Click(object? sender, RoutedEventArgs e)
    {
        //if collapsed
        if (LayoutMain.RowDefinitions[0].Height.Value == 4)
        {
            LayoutMain.RowDefinitions = new("*,*");
        }
        else
        {
            LayoutMain.RowDefinitions = new("4*,*");
        }
    }
}

public enum Layout
{
    Wide,
    Narrow
}
