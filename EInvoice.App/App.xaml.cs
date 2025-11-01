using System.IO;
using System.Windows;
using EInvoice.Lib.MailClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EInvoice.App;

public partial class App
{
    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddUserSecrets<App>();
            config.AddJsonFile("appsettings.json", false, true);
            config.AddJsonFile("data/customers.json", false, true);
            config.AddJsonFile("data/invoice-template.json", false, true);
        })
        .ConfigureServices((context, services) =>
        {
            services.ConfigureSmtpClient(context.Configuration);
            services.ConfigureEInvoiceView(context.Configuration);
        })
        .Build();


    protected override void OnStartup(StartupEventArgs e)
    {
        Host.StartAsync().GetAwaiter().GetResult();
        var mainWindow = Host.Services.GetRequiredService<EInvoiceView>();
        mainWindow.Show();
        base.OnStartup(e);
    }


    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            Host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }
        finally
        {
            Host.Dispose();
        }

        base.OnExit(e);
    }
}