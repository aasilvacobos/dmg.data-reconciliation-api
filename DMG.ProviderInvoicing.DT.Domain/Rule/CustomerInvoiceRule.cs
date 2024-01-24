namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class CustomerInvoiceRule
{
    public static bool IsAllLinesCreated(CustomerInvoice customerInvoice) =>
        customerInvoice.ItemLineLineNumber == customerInvoice.MaxItemCount;
}