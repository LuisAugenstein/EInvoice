# EInvoice

Simple application to create Zugferd Invoices and automatically send them per Mail.

Install [pdf24](https://www.pdf24.org/de/) locally on your system. The following command is invoked to generate the electronic invoice based on a json template.
```
& "C:\Program Files\PDF24\pdf24-Toolbox.exe" -createInvoice ".\invoice.json" ".\invoice.pdf" -outputType zugferd:xrechnung
```
The main functionality of this project is to simplify the creation of the json template and to send the generated invoice per mail.

## Publish
```bash
dotnet publish
```
Generate a standalone executable of the application at `EInvoice.App/bin/Release/EInvoiceApp.exe`. 
The resulting file can be deployed to any location on your system, for example: `C:\Program Files\EInvoiceApp`.
The application requires the following accompanying files:
1. **`appsettings.json`** – Stores application configuration, including SMTP credentials for sending emails.
2. **`data/invoice-template.json`** – Constant parts of the invoice. You can create or export this template via [PDF24’s electronic invoice tool](https://tools.pdf24.org/de/elektronische-rechnung-erstellen).
3. **`data/customers.json`** – list of customer-specific data that will be selectable in the application’s dropdown.