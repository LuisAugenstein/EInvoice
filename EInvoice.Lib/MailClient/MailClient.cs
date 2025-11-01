using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EInvoice.Lib.MailClient;

public class MailClient(IOptions<MailClientOptions> options, ISmtpClient smtpClient) : IMailClient
{
    private readonly MailClientOptions _options = options.Value;

    public async Task SendAsync(string subject, string body, List<string> attachmentPaths, CancellationToken cancellationToken = default)
    {
        await smtpClient.ConnectAsync(_options.Server, _options.Port, useSsl: true, cancellationToken);
        await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

        var bodyBuilder = new BodyBuilder()
        {
            TextBody = body
        };

        // TODO: add attachments to body

        var message = new MimeMessage()
        {
            Subject = subject,
            Body = bodyBuilder.ToMessageBody()
        };
        await smtpClient.SendAsync(message, cancellationToken);
    }
}
