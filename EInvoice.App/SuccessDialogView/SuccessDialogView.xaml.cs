namespace EInvoice.App;

public partial class SuccessDialogView
{
    public SuccessDialogView(SuccessDialogViewModel successDialogViewModel)
    {
        InitializeComponent();
        DataContext = successDialogViewModel;
    }
}