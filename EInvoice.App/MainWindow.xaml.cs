using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EInvoice.Lib;

namespace EInvoice.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ObservableCollection<Customer> _customers = new();
    private ObservableCollection<Position> _positions = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeData();
        SetupBindings();
    }

    private void InitializeData()
    {
        // Initialize sample customers
        _customers.Add(new Customer { Name = "Musterfirma GmbH" });
        _customers.Add(new Customer { Name = "Beispiel AG" });
        _customers.Add(new Customer { Name = "Demo KG" });
        
        // Set default date to today
        InvoiceDatePicker.SelectedDate = DateTime.Today;
    }

    private void SetupBindings()
    {
        // Bind customers to the ComboBox
        CustomerComboBox.ItemsSource = _customers;
        
        // Set up data binding for the fields
        InvoiceNumberTextBox.DataContext = this;
        InvoiceDatePicker.DataContext = this;
        CustomerComboBox.DataContext = this;
    }

    private void AddPositionButton_Click(object sender, RoutedEventArgs e)
    {
        AddNewPosition();
    }

    private void AddNewPosition()
    {
        // Create a new position panel
        var positionPanel = new Border
        {
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(10)
        };

        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

        // Name field
        var nameLabel = new Label { Content = "Name:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
        var nameTextBox = new TextBox 
        { 
            Width = 150, 
            Margin = new Thickness(0, 0, 10, 0),
            VerticalContentAlignment = VerticalAlignment.Center
        };

        // Quantity field
        var quantityLabel = new Label { Content = "Anzahl:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
        var quantityTextBox = new TextBox 
        { 
            Width = 80, 
            Margin = new Thickness(0, 0, 10, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = "1"
        };
        quantityTextBox.PreviewTextInput += QuantityTextBox_PreviewTextInput;

        // Amount field
        var amountLabel = new Label { Content = "Betrag:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
        var amountTextBox = new TextBox 
        { 
            Width = 100, 
            Margin = new Thickness(0, 0, 10, 0),
            VerticalContentAlignment = VerticalAlignment.Center
        };
        amountTextBox.PreviewTextInput += AmountTextBox_PreviewTextInput;

        // Remove button
        var removeButton = new Button { Content = "-", Width = 30, Height = 30 };
        removeButton.Click += (s, e) => PositionsPanel.Children.Remove(positionPanel);

        // Add controls to the panel
        stackPanel.Children.Add(nameLabel);
        stackPanel.Children.Add(nameTextBox);
        stackPanel.Children.Add(quantityLabel);
        stackPanel.Children.Add(quantityTextBox);
        stackPanel.Children.Add(amountLabel);
        stackPanel.Children.Add(amountTextBox);
        stackPanel.Children.Add(removeButton);

        positionPanel.Child = stackPanel;
        PositionsPanel.Children.Add(positionPanel);
    }

    private void InvoiceNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Only allow numeric input
        e.Handled = !IsTextAllowed(e.Text);
    }

    private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Only allow numeric input
        e.Handled = !IsTextAllowed(e.Text);
    }

    private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow numeric and decimal point
        e.Handled = !IsDecimalTextAllowed(e.Text);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Close the window when Abbrechen is clicked
        this.Close();
    }

    private static bool IsTextAllowed(string text)
    {
        return int.TryParse(text, out _);
    }

    private static bool IsDecimalTextAllowed(string text)
    {
        return decimal.TryParse(text, out _) || text == "." || text == ",";
    }
}