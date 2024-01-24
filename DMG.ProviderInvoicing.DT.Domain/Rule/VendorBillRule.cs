using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class VendorBillRule
{
    /// Should a vendor bill lines be sent to Finance (i.e., inserted into FAL)
    /// TODO relocate to ProviderInvoiceRule when we can determine this by examining the provider invoice instead of vendor bill
    public static bool ShouldSendToFinance(Lst<VendorBillLineInsert> vendorBillLineInserts) =>
        vendorBillLineInserts.Count > 0;
}