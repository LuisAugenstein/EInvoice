using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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

        // Quantity field (using TextBox for now, will improve later)
        var quantityLabel = new Label { Content = "Anzahl:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
        var quantityTextBox = new TextBox 
        { 
            Width = 80, 
            Margin = new Thickness(0, 0, 10, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = "1",
            HorizontalContentAlignment = HorizontalAlignment.Right
        };
        quantityTextBox.PreviewTextInput += QuantityTextBox_PreviewTextInput;

        // Amount field with Euro symbol
        var amountLabel = new Label { Content = "Betrag (€):", Width = 80, VerticalAlignment = VerticalAlignment.Center };
        var amountTextBox = new TextBox 
        { 
            Width = 100, 
            Margin = new Thickness(0, 0, 10, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Right
        };
        amountTextBox.PreviewTextInput += AmountTextBox_PreviewTextInput;

        // Remove button with trash icon simulation
        var removeButton = new Button 
        { 
            Content = "🗑", 
            Width = 30, 
            Height = 30,
            Background = Brushes.Red,
            Foreground = Brushes.White,
            ToolTip = "Position entfernen",
            FontWeight = FontWeights.Bold
        };
        removeButton.Click += (s, e) => RemovePosition(positionPanel);

        // Add controls to the panel
        stackPanel.Children.Add(nameLabel);
        stackPanel.Children.Add(nameTextBox);
        stackPanel.Children.Add(quantityLabel);
        stackPanel.Children.Add(quantityTextBox);
        stackPanel.Children.Add(amountLabel);
        stackPanel.Children.Add(amountTextBox);
        stackPanel.Children.Add(removeButton);

        positionPanel.Child = stackPanel;
        
        // Add the new position before the add button
        PositionsPanel.Children.Add(positionPanel);
    }

    private void RemovePosition(Border positionPanel)
    {
        // Remove the position from the panel
        PositionsPanel.Children.Remove(positionPanel);
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

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the path to the invoice-template.json file in the application directory
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice-template.json");
            
            // Check if the template file exists
            if (File.Exists(templatePath))
            {
                // Read the JSON template file content
                string jsonContent = File.ReadAllText(templatePath);
                
                // Create the output invoice.json file path
                string invoicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice.json");
                
                // Write the template content to invoice.json
                File.WriteAllText(invoicePath, jsonContent);
                
                // Define the path to pdf24-Toolbox.exe
                string pdf24ToolboxPath = @"C:\Program Files\PDF24\pdf24-Toolbox.exe";
                
                // Check if pdf24-Toolbox.exe exists
                if (File.Exists(pdf24ToolboxPath))
                {
                    // Create the process start info for pdf24-Toolbox.exe
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = pdf24ToolboxPath,
                        Arguments = $"-createInvoice \"{invoicePath}\" \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice.pdf")}\" -outputType zugferd:xrechnung",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    
                    // Start the process
                    using (Process? process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            // Wait for the process to finish
                            process.WaitForExit();
                            
                            // Check the exit code
                            if (process.ExitCode == 0)
                            {
                                MessageBox.Show("Invoice PDF has been successfully created.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                string errorOutput = process.StandardError.ReadToEnd();
                                MessageBox.Show($"Error creating invoice PDF. Exit code: {process.ExitCode}\nError: {errorOutput}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to start pdf24-Toolbox.exe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("pdf24-Toolbox.exe not found at the expected location.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Invoice template file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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