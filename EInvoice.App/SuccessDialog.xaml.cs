using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace EInvoice.App
{
    public partial class SuccessDialog : Window
    {
        private string _pdfPath;

        public SuccessDialog(string pdfPath)
        {
            InitializeComponent();
            _pdfPath = pdfPath;
            PdfPathLinkText.Text = _pdfPath;
        }

        private void PdfPathHyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the PDF file with the default application
                Process.Start(new ProcessStartInfo(_pdfPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen der PDF-Datei: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendEmailButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, just print to console
            Console.WriteLine("Versende Email");
            
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}