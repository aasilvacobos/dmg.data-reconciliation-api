using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Validation;

/// Validations related to a vendor bill
public static class VendorBillValidator
{
    public static bool IsTotalAmountNegative(Lst<VendorBillLineInsert> vendorBillLineInserts) =>
        vendorBillLineInserts
            .Map(vendorBillLineInsert => vendorBillLineInsert.PurchaseItemLineAmount)
            .Sum() < 0.0M;

    /// Validate a collection of VendorBillLineInserts. Since PI is not the SOR for any of entities used to
    /// create the vendor bill, no validations should be necessary. But after a serious incident occurred
    /// with missing data, we decided to add another level here of core validations.
    public static Validation<ErrorMessage, Lst<VendorBillLineInsert>> Validate(Lst<VendorBillLineInsert> vendorBillLineInserts, JobBillingId jobBillingId)
    {
        var totalAmountValidation = 
            (IsTotalAmountNegative(vendorBillLineInserts)
                ? Either<ErrorMessage, Unit>.Left(ErrorMessage.NewVendorBillTotalAmountIsNegative(jobBillingId.Value))
                : Either<ErrorMessage, Unit>.Right(Unit.Default))
            .ToValidation();
        // add additional validations as necessary...

        //TODO figure out how to handle multiple validations
        return (totalAmountValidation)
            .Apply((totalAmountValid) =>
                totalAmountValid.Map(_ => vendorBillLineInserts));
    }    
}