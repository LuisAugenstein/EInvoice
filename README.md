# EInvoice

Application to create Zugferd Invoices and automatically send them per Mail.

Install [pdf24](https://www.pdf24.org/de/) locally on your system. 
```
& "C:\Program Files\PDF24\pdf24-Toolbox.exe" -createInvoice ".\invoice.json" ".\invoice.pdf" -outputType zugferd:xrechnung
```

## Export 
Publish the application.
```bash
dotnet publish
```
This builds the application as a standalone executable `EInvoice.App/bin/Release/EInvoiceApp.exe` which can be copied to any destination on your file system, e.g., `C:\Program Files\EInvoiceApp`.
The application expects an `invoice-template.json` next to the executable. You can obtain this template by exporting your invoice configuration from [https://tools.pdf24.org/de/elektronische-rechnung-erstellen](https://tools.pdf24.org/de/elektronische-rechnung-erstellen). 
Additionally, you need to provide a `customer-data.json` that contains a list of customer specific options for the invoice that will be selectable in a dropdown. Last a `secrets.json` needs to be provided which includes SMTP credentials. 