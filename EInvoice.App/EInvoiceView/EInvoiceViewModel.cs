using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EInvoice.Lib.MailClient;
using EInvoice.Lib.Models;
using Microsoft.Extensions.Options;

namespace EInvoice.App;

public partial class EInvoiceViewModel(
    IOptions<EInvoiceOptions> options,
    IEInvoiceService eInvoiceService,
    IMailClient mailClient)
    : ObservableObject
{
    private readonly EInvoiceOptions _options = options.Value;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CreateEInvoiceCommand))]
    private bool _creatingEInvoice;

    [ObservableProperty] private DateTime _invoiceDate = DateTime.Today;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CreateEInvoiceCommand))]
    private int? _invoiceNumber;

    [ObservableProperty] private ObservableCollection<PositionViewModel> _positions = [];

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CreateEInvoiceCommand))]
    private string? _selectedCustomerName;

    public IReadOnlyList<string> CustomerNames => options.Value.Customers.Select(c => c.Name).ToList();
    private Customer? SelectedCustomer => _options.Customers.FirstOrDefault(c => c.Name == SelectedCustomerName);
    private bool CanCreateEInvoice => !CreatingEInvoice && SelectedCustomer is not null && InvoiceNumber is not null;

    private void ResetForm()
    {
        SelectedCustomerName = null;
        InvoiceNumber = null;
        InvoiceDate = DateTime.Today;
        Positions.Clear();
    }

    [RelayCommand]
    private void AddPosition()
    {
        Positions.Add(new PositionViewModel
        {
            Name = "",
            Quantity = 1,
            NetUnitPrice = 0
        });
    }

    [RelayCommand]
    private void DeletePosition(PositionViewModel position)
    {
        Positions.Remove(position);
    }

    [RelayCommand(CanExecute = nameof(CanCreateEInvoice))]
    private async Task CreateEInvoice()
    {
        CreatingEInvoice = true;
        var positions = Positions.Select(p => new Position(p.Name, p.Quantity, p.NetUnitPrice)).ToList();
        var eInvoice = new Lib.Models.EInvoice((int)InvoiceNumber!, SelectedCustomer!, InvoiceDate, positions);
        try
        {
            var pdf24EinvoicePdfPath = await eInvoiceService.GenerateEInvoice(eInvoice);
            var successDialogViewModel = new SuccessDialogViewModel(mailClient)
            {
                EInvoicePdfPath = pdf24EinvoicePdfPath,
                CustomerEmail = SelectedCustomer!.Email
            };
            var dialog = new SuccessDialogView(successDialogViewModel)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            dialog.ShowDialog();
            ResetForm();
        }
        catch (Exception e)
        {
            MessageBox.Show($"Fehler beim Verarbeiten der E-Rechnung: {e.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        CreatingEInvoice = false;
    }
}

public partial class PositionViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private decimal _netUnitPrice;
    [ObservableProperty] private int _quantity;
}