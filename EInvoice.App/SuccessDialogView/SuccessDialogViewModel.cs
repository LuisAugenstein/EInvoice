using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EInvoice.App;

public partial class SuccessDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _customerEmail = string.Empty;
    [ObservableProperty] private string _eInvoicePdfPath = string.Empty;

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
    private void SendEmail()
    {
        // try
        // {
        //     var secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets.json");
        //     if (!File.Exists(secretsPath))
        //     {
        //         MessageBox.Show("SMTP-Konfiguration nicht gefunden.", "Fehler",
        //             MessageBoxButton.OK, MessageBoxImage.Warning);
        //         return;
        //     }
        //
        //     var secretsJson = await File.ReadAllTextAsync(secretsPath);
        //     var smtpSettings = SmtpSettings.LoadFromJson(secretsJson);
        //     var emailService = new EmailService(smtpSettings);
        //
        //     // Example send call (uncomment when EmailService implemented)
        //     // bool sent = await emailService.SendInvoiceEmailAsync(RecipientEmail, PdfPath);
        //
        //     MessageBox.Show($"E-Mail erfolgreich an {CustomerEmail} gesendet.", "Erfolg",
        //         MessageBoxButton.OK, MessageBoxImage.Information);
        // }
        // catch (Exception ex)
        // {
        //     MessageBox.Show($"Fehler beim Senden der E-Mail: {ex.Message}", "Fehler",
        //         MessageBoxButton.OK, MessageBoxImage.Error);
        // }
    }

    [RelayCommand]
    private static void Close(Window window)
    {
        window?.Close();
    }
}