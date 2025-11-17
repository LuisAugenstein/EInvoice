namespace EInvoice.Lib.MailClient;

public interface IMailClient
{
    Task SendAsync(string toAddress, string subject, string body, List<string>? attachmentPaths = null,
        CancellationToken cancellationToken = default);
}