using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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
    private List<CustomerData> _customerData = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeData();
        SetupBindings();
    }

    private void InitializeData()
    {
        // Load customers from customer-data.json
        LoadCustomerData();
        
        // Set default date to today
        InvoiceDatePicker.SelectedDate = DateTime.Today;
    }

    private void LoadCustomerData()
    {
        try
        {
            // Get the path to the customer-data.json file in the application directory
            string customerDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customer-data.json");
            
            // Check if the customer data file exists
            if (File.Exists(customerDataPath))
            {
                // Read the JSON file content
                string jsonContent = File.ReadAllText(customerDataPath);
                
                // Load customer data
                _customerData = CustomerData.LoadFromJson(jsonContent);
                
                // Convert to Customer objects and add to the collection
                foreach (var customerData in _customerData)
                {
                    _customers.Add(new Customer 
                    { 
                        Name = customerData.Name 
                    });
                }
            }
            else
            {
                // Initialize sample customers if file doesn't exist
                _customers.Add(new Customer { Name = "Musterfirma GmbH" });
                _customers.Add(new Customer { Name = "Beispiel AG" });
                _customers.Add(new Customer { Name = "Demo KG" });
            }
        }
        catch
        {
            // Initialize sample customers if there's an error
            _customers.Add(new Customer { Name = "Musterfirma GmbH" });
            _customers.Add(new Customer { Name = "Beispiel AG" });
            _customers.Add(new Customer { Name = "Demo KG" });
        }
    }

    private string GetCustomerElectronicAddress(string customerName)
    {
        // Try to find the customer in the customer data
        var customer = _customerData.Find(c => c.Name == customerName);
        return customer?.ElectronicAddress ?? "customer@example.com";
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

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Show progress indicator
        ProgressPanel.Visibility = Visibility.Visible;
        SaveButton.IsEnabled = false;
        
        try
        {
            // Create UserInput object with user data
            var userInput = new UserInput
            {
                Number = InvoiceNumberTextBox.Text,
                IssueDate = InvoiceDatePicker.SelectedDate ?? DateTime.Now,
                Buyer = new Buyer(),
                Positions = CollectPositionsData()
            };
            
            // Set buyer information if a customer is selected
            if (CustomerComboBox.SelectedItem is Customer selectedCustomer)
            {
                userInput.Buyer.Name = selectedCustomer.Name;
                
                // Try to find the electronic address for the selected customer
                string electronicAddress = GetCustomerElectronicAddress(selectedCustomer.Name);
                userInput.Buyer.ElectronicAddress = electronicAddress;
            }
            
            // Get the path to the invoice-template.json file in the application directory
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice-template.json");
            
            // Check if the template file exists
            if (File.Exists(templatePath))
            {
                // Read the JSON template file content
                string jsonContent = File.ReadAllText(templatePath);
                
                // Update the template with user inputs
                string updatedJsonContent = userInput.UpdateInvoiceTemplate(jsonContent);
                
                // Create the output invoice.json file path
                string invoicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice.json");
                
                // Write the updated content to invoice.json
                File.WriteAllText(invoicePath, updatedJsonContent);
                
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
                    
                    // Run the process asynchronously
                    bool success = await Task.Run(() =>
                    {
                        try
                        {
                            // Start the process
                            using (Process? process = Process.Start(startInfo))
                            {
                                if (process != null)
                                {
                                    // Wait for the process to finish
                                    process.WaitForExit();
                                    
                                    // Check the exit code
                                    return process.ExitCode == 0;
                                }
                            }
                        }
                        catch
                        {
                            // Ignore exceptions in the task, they'll be handled in the main thread
                        }
                        return false;
                    });
                    
                    // Hide progress indicator
                    ProgressPanel.Visibility = Visibility.Collapsed;
                    SaveButton.IsEnabled = true;
                    
                    if (success)
                    {
                        // Create the path to the generated PDF
                        string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invoice.pdf");
                        
                        // Show the custom success dialog
                        SuccessDialog dialog = new SuccessDialog(pdfPath);
                        dialog.Owner = this;
                        dialog.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Fehler beim Erstellen der E-Rechnung. Bitte überprüfen Sie, ob PDF24 Toolbox korrekt installiert ist.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Hide progress indicator
                    ProgressPanel.Visibility = Visibility.Collapsed;
                    SaveButton.IsEnabled = true;
                    
                    MessageBox.Show("pdf24-Toolbox.exe wurde nicht am erwarteten Speicherort gefunden. Bitte stellen Sie sicher, dass PDF24 Toolbox installiert ist.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                // Hide progress indicator
                ProgressPanel.Visibility = Visibility.Collapsed;
                SaveButton.IsEnabled = true;
                
                MessageBox.Show("Die Rechnungsvorlage wurde nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            // Hide progress indicator
            ProgressPanel.Visibility = Visibility.Collapsed;
            SaveButton.IsEnabled = true;
            
            MessageBox.Show($"Fehler beim Verarbeiten der E-Rechnung: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

    private List<InvoicePosition> CollectPositionsData()
    {
        var positions = new List<InvoicePosition>();
        
        // Iterate through all the position panels
        foreach (var child in PositionsPanel.Children)
        {
            if (child is Border positionBorder && positionBorder.Child is StackPanel stackPanel)
            {
                // Extract data from the controls in the stack panel
                var nameTextBox = stackPanel.Children[1] as TextBox; // Name textbox is at index 1
                var quantityTextBox = stackPanel.Children[3] as TextBox; // Quantity textbox is at index 3
                var amountTextBox = stackPanel.Children[5] as TextBox; // Amount textbox is at index 5
                
                // Create a new InvoicePosition object with the extracted data
                var position = new InvoicePosition
                {
                    Name = nameTextBox?.Text ?? string.Empty,
                    Quantity = int.TryParse(quantityTextBox?.Text, out int quantity) ? quantity : 0,
                    NetUnitPrice = decimal.TryParse(amountTextBox?.Text, out decimal amount) ? amount : 0
                };
                
                positions.Add(position);
            }
        }
        
        return positions;
    }
}