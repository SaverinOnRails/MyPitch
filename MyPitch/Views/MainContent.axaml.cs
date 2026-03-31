using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

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
            Debug.WriteLine("Buidling wide layout");
            LayoutMain.ColumnDefinitions = new("*,Auto");
            Grid.SetColumn(CircleHaloEffect, 0);
            Grid.SetColumn(Circle, 0);
            Grid.SetColumn(Card, 1);
            Card.MinWidth = 400;
        }
        else
        {
            Debug.WriteLine("bUIDLING ANRROW LAYOUT");
            LayoutMain.RowDefinitions = new("*,Auto");
            Grid.SetRow(CircleHaloEffect, 0);
            Grid.SetRow(Circle, 0);
            Grid.SetRow(Card, 1);
            Card.MinWidth = 250;
        }
    }
    public Layout Layout { get; set; }
}

public enum Layout
{
    Wide,
    Narrow
}