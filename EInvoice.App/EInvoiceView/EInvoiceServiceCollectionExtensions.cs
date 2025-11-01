using System.IO;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EInvoice.App;

public static class EInvoiceServiceCollectionExtensions
{
    public static void ConfigureEInvoiceView(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EInvoiceOptions>(options =>
        {
            var section = configuration.GetSection("EInvoiceOptions");
            section.Bind(options);
            var pdf24EInvoiceTemplateJsonPath = section.GetSection("Pdf24EInvoiceTemplateJsonPath").Value;
            if (string.IsNullOrWhiteSpace(pdf24EInvoiceTemplateJsonPath))
                throw new InvalidOperationException("Pdf24EInvoiceTemplateJsonPath is not configured.");
            if (!File.Exists(pdf24EInvoiceTemplateJsonPath))
                throw new FileNotFoundException("PDF24 JSON template not found", pdf24EInvoiceTemplateJsonPath);

            var jsonText = File.ReadAllText(pdf24EInvoiceTemplateJsonPath);
            options.Pdf24EInvoiceTemplateJson = (JsonObject)JsonNode.Parse(jsonText)!;
        });
        services.AddTransient<IEInvoiceService, EInvoiceService>();
        services.AddTransient<EInvoiceView>();
        services.AddSingleton<EInvoiceViewModel>();
    }
}