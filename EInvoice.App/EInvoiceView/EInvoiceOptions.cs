using System.Text.Json.Nodes;
using EInvoice.Lib.Models;

namespace EInvoice.App;

public class EInvoiceOptions
{
    public string Pdf24ToolboxPath { get; set; } = string.Empty;
    public JsonObject Pdf24EInvoiceTemplateJson { get; set; } = new();
    public List<Customer> Customers { get; set; } = [];
}