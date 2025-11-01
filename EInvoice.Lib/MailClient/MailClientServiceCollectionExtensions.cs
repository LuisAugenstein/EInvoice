using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MailKit.Net.Smtp;

namespace EInvoice.Lib.MailClient;

public static class MailClientServiceCollectionExtensions
{
    public static void ConfigureSmtpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailClientOptions>(configuration.GetSection(nameof(MailClientOptions)));
        services.AddTransient<ISmtpClient, SmtpClient>();
        services.AddTransient<IMailClient, MailClient>();
    }
}
