namespace EInvoice.App;

public partial class EInvoiceView
{
    public EInvoiceView(EInvoiceViewModel eInvoiceViewModel)
    {
        InitializeComponent();
        DataContext = eInvoiceViewModel;
    }
}