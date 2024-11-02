using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gomory_HuCalculator.GomoryHu;

namespace Gomory_HuCalculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _nodeCount;

    private int NodeCount
    {
        get => _nodeCount;
        set => _nodeCount = value > 2 ? value : 2;
    }
    
    public MainWindow()
    {
        NodeCount = 7;
        
        InitializeComponent();
        GenerateWeightMatrixGrid();
        PreDefineGraph();
    }
    
    private void GenerateWeightMatrixGrid()
    {
        TransportGrid ??= new Grid();
        TransportGrid.RowDefinitions.Clear();
        TransportGrid.ColumnDefinitions.Clear();
        TransportGrid.Children.Clear();
    
        // Create Row Definitions
        for (var i = 0; i <= NodeCount; i++)
        {
            TransportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            TransportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
    
        // Create headers (top row and left column)
        for (var i = 1; i <= NodeCount; i++)
        {
            var rowHeaders = new TextBlock
            {
                Text = ((char)('a' + i - 1)).ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(rowHeaders, i);
            Grid.SetColumn(rowHeaders, 0);
            TransportGrid.Children.Add(rowHeaders);
            
            var columnHeaders = new TextBlock
            {
                Text = ((char)('a' + i - 1)).ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(columnHeaders, 0);
            Grid.SetColumn(columnHeaders, i);
            TransportGrid.Children.Add(columnHeaders);
        }
    
        // Fill grid cells with TextBoxes for input (except the first row/column for headers)
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                var cell = new TextBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    MinWidth = 30,
                    MinHeight = 30,
                };
    
                // Attach the validation event handler to each TextBox
                cell.PreviewTextInput += Cell_PreviewTextInput;
    
                Grid.SetRow(cell, i);
                Grid.SetColumn(cell, j);
                TransportGrid.Children.Add(cell);
            }
        }
    }
    
    private void Cell_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9]+"); // Only positive digits are allowed
        e.Handled = regex.IsMatch(e.Text);
    }
    
    private void PlusButton_OnClick(object sender, RoutedEventArgs e)
    {
        NodeCount++;
        GenerateWeightMatrixGrid();
    }
    private void MinusButton_OnClick(object sender, RoutedEventArgs e)
    {
        NodeCount--;
        GenerateWeightMatrixGrid();
    }

    private void SolveButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Read weight matrix from the grid
        var weightMatrix = new int[NodeCount, NodeCount];
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                var cell = (TextBox)TransportGrid.Children
                    .Cast<UIElement>()
                    .FirstOrDefault(element => Grid.GetRow(element) == i && Grid.GetColumn(element) == j)!;
                weightMatrix[i - 1, j - 1] = int.Parse(cell?.Text ?? string.Empty);
            }
        }

        var gomoryHuAlgorithm = new GomoryHuAlgorithm(weightMatrix);
        gomoryHuAlgorithm.Solve();
    }
    
    private void PreDefineGraph()
    {
        // 24-th variant weight matrix
        // int [,]weightMatrix =
        // {
        //     {0, 2, 8, 0, 1, 4, 0, 0},
        //     {2, 0, 0, 0, 4, 7, 0, 0},
        //     {8, 0, 0, 7, 0, 3, 0, 0},
        //     {0, 0, 7, 0, 0, 0, 6, 0},
        //     {1, 4, 0, 0, 0, 0, 0, 2},
        //     {4, 7, 3, 0, 0, 0, 0, 4},
        //     {0, 0, 0, 6, 0, 0, 0, 6},
        //     {0, 0, 0, 0, 2, 4, 6, 0}
        // };
        
        // Demonstration weight matrix
        int [,]weightMatrix =
        {
            {0, 8, 9, 7, 0, 0, 0},
            {8, 0, 0, 5, 7, 0, 0},
            {9, 0, 0, 4, 0, 9, 0},
            {7, 5, 4, 0, 4, 6, 8},
            {0, 7, 0, 4, 0, 0, 2},
            {0, 0, 9, 6, 0, 0, 11},
            {0, 0, 0, 8, 2, 11, 0}
        };
        
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                ((TextBox)GetGridElement(TransportGrid, i, j)).Text = weightMatrix[i - 1, j - 1].ToString();
            }
        }
    }
    
    private static UIElement GetGridElement(Grid grid, int row, int column)
    {
        foreach (UIElement element in grid.Children)
        {
            if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
                return element;
        }

        return null;
    }
}