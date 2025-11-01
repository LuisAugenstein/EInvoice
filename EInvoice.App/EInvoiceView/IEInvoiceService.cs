namespace EInvoice.App;

public interface IEInvoiceService
{
    public Task<string> GenerateEInvoice(Lib.Models.EInvoice eInvoice);
}