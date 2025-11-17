using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EInvoice.Lib.MailClient;

namespace EInvoice.App;

public partial class SuccessDialogViewModel(IMailClient mailClient) : ObservableObject
{
    [ObservableProperty] private string _customerEmail = string.Empty;
    [ObservableProperty] private string _eInvoicePdfPath = string.Empty;
    public string MailSubject { get; set; } = "Rechnung";

    public string MailBody { get; set; } = """
                                           Sehr geehrter Kunde,

                                           anbei erhalten Sie Ihre Rechnung.

                                           Mit freundlichen Grüßen,
                                           Jürgen Augenstein
                                           """;

    [RelayCommand]
    private void OpenPdf()
    {
        try
        {
            Process.Start(new ProcessStartInfo(EInvoicePdfPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Öffnen der PDF-Datei: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SendEmailAsync(Window window)
    {
        try
        {
            await mailClient.SendAsync(CustomerEmail, MailSubject, MailBody, [EInvoicePdfPath]);
            window?.Close();
            MessageBox.Show($"Die Mail wurde erfolgreich an {CustomerEmail} versendet.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Versenden der E-Mail: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private static void Close(Window window)
    {
        window?.Close();
    }
}