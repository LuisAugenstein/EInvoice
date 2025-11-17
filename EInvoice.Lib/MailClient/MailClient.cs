using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EInvoice.Lib.MailClient;

public class MailClient(IOptions<MailClientOptions> options, ISmtpClient smtpClient) : IMailClient
{
    private readonly MailClientOptions _options = options.Value;

    public async Task SendAsync(string toAddress, string subject, string body, List<string>? attachmentPaths = null,
        CancellationToken cancellationToken = default)
    {
        var bodyBuilder = new BodyBuilder
        {
            TextBody = body
        };
        foreach (var path in attachmentPaths ?? []) await bodyBuilder.Attachments.AddAsync(path, cancellationToken);

        var message = new MimeMessage
        {
            Subject = subject,
            Body = bodyBuilder.ToMessageBody()
        };
        message.From.Add(MailboxAddress.Parse(_options.Username));
        message.To.Add(MailboxAddress.Parse(toAddress));

        await smtpClient.ConnectAsync(_options.SmtpServer, _options.Port, SecureSocketOptions.StartTls,
            cancellationToken);
        await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        await smtpClient.SendAsync(message, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}