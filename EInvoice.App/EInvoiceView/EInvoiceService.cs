using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace EInvoice.App;

public class EInvoiceService(IOptions<EInvoiceOptions> options) : IEInvoiceService
{
    private readonly EInvoiceOptions _options = options.Value;

    public async Task<string> GenerateEInvoice(Lib.Models.EInvoice eInvoice)
    {
        var pdf24EInvoiceJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdf24Invoice.json");
        var pdf24EInvoiceJson = eInvoice.ToPdf24Json(_options.Pdf24EInvoiceTemplateJson.ToJsonString(
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }));
        await File.WriteAllTextAsync(pdf24EInvoiceJsonPath, pdf24EInvoiceJson);

        if (!File.Exists(_options.Pdf24ToolboxPath))
            throw new InvalidOperationException(
                "Fehler beim Erstellen der E-Rechnung." +
                $" Bitte überprüfen Sie, ob PDF24 Toolbox {_options.Pdf24ToolboxPath} korrekt installiert ist.");

        var pdf24EinvoicePdfPath = pdf24EInvoiceJsonPath.Replace(".json", ".pdf");
        await RunCommand(_options.Pdf24ToolboxPath,
            $"-createInvoice \"{pdf24EInvoiceJsonPath}\" \"{pdf24EinvoicePdfPath}\" -outputType zugferd:xrechnung");
        return pdf24EinvoicePdfPath;
    }

    private static async Task RunCommand(string executable, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            throw new InvalidOperationException($"Failed to execute {executable} {arguments}");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new Exception($"PDF24 failed with code {process.ExitCode}: {error}");
    }
}