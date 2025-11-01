namespace EInvoice.Lib.MailClient;

public interface IMailClient
{
    Task SendAsync(string subject, string body, List<string> attachmentPaths, CancellationToken cancellationToken = default);
}
