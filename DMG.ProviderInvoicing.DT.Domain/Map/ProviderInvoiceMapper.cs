using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using DMG.ProviderInvoicing.DT.Domain.Validation;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Map; 

/// Contains invoice line-level data required on the vendor bill insert. This is the data
/// that is not on the invoice "header". Structure is currently just used for mapping. 
public record VendorBillLineInsertLineLevelData(
    FalItemType             FalItemType,
    decimal                 Quantity,               // line_quantity
    decimal                 Rate,                   // line_rate
    decimal                 Amount,                 // line_amount
    // optionals
    Option<NonEmptyText>    RateTypeText,           // rate_type (labor and trip charge only)
    Option<decimal>         LaborQuantityInMinutes, // labor_quantity_minutes
    Option<NonEmptyText>    Description,            // line_description
    Option<NonEmptyText>    ItemReference);         // line_item_ref

/// Functions to map the provider invoice to other entities
public static class ProviderInvoiceMapper
{
    public static Tuple<PaymentTermsIdentifier,bool,NonEmptyText> ToPrepaidData(bool isPaidByCreditCard)
    {
        // TODO Use lookup data to map payment terms (string) to identifier (uint). This will set identifier for any type of payment terms.
        var paymentTermsIdentifier = 
            isPaidByCreditCard
                ? PaymentTermsRule.IdentifierCreditCard
                : PaymentTermsRule.IdentifierNet55;
        
        var isPrepaid = PaymentTermsRule.ToPrePaymentFlag(paymentTermsIdentifier); 
        var billStatus = NonEmptyText.NewUnsafe(isPrepaid ? "Approved" : "Pending Approval");
        return new (paymentTermsIdentifier, isPrepaid, billStatus);
    }

    public static NonEmptyText ToPurchaseItemLineReference(bool isPrepaid, NonEmptyText itemReference) => 
        isPrepaid
            ? NonEmptyText.NewUnsafe($"{itemReference.Value}-Prepaid")
            : itemReference;
    
    public static NonEmptyText ToVendorBillStatus(ProviderBilling providerBilling)
    {
        //https://divisionsinc.atlassian.net/wiki/spaces/DP/pages/2098921509/Snow+in+Pro+-+Provider+Invoice
        //todo: for routine, if provider is on Per Occurnce Contract (in the PSA) and the customer is on Seasonal contract, then make Approved
        var (_, isPrepaid, _) = ToPrepaidData(providerBilling.Payment.IsPaidByCreditCard);        
        return NonEmptyText.NewUnsafe(isPrepaid ? "Approved" : "Pending Approval");
    }
    
    public static VendorBillLineInsert ToVendorBillLineInsert(ProviderInvoiceInsert providerInvoice, VendorBillLineInsertLineLevelData lineLevelData)
    {
        var (paymentTermsIdentifier, isPrepaid, billStatus) = ToPrepaidData(providerInvoice.Payment.IsPaidByCreditCard);
        var purchaseItemLineReference = lineLevelData.ItemReference.Map(l => ToPurchaseItemLineReference(isPrepaid, l));
        
        return new VendorBillLineInsert(
            providerInvoice.ProviderInvoiceId,
            providerInvoice.TicketNumber,
            providerInvoice.TicketId,
            providerInvoice.ProviderOrgId,
            TransactionDate: providerInvoice.TransactionDate, 
            PaymentDueDate: providerInvoice.TransactionDate.AddDays(PaymentTermsRule.ToPaymentDueNetDays(paymentTermsIdentifier)),
            providerInvoice.JobSummary,
            providerInvoice.PropertyId,
            providerInvoice.ServiceLineName,
            providerInvoice.JobWorkNumber,
            providerInvoice.JobWorkId,
            providerInvoice.JobBillingId,
            DmgInvoiceNumber:providerInvoice.DmgInvoiceNumber,
            ContractType:NonEmptyText.NewUnsafe("Non-Routine"),
            Currency:NonEmptyText.NewUnsafe("USD"),
            DmgProviderInvoiceStatus:NonEmptyText.NewUnsafe("Pending"),
            BillStatus:billStatus,            
            PaymentTermsReference:paymentTermsIdentifier.Value,
            IsCreditCardPayment:providerInvoice.Payment.IsPaidByCreditCard, // specifically driven by CC payment, not any prepayment
            IsHold: providerInvoice.Payment.IsPaidByCreditCard,             // specifically driven by CC payment, not any prepayment 
            IsPrepaid: isPrepaid,
            lineLevelData.FalItemType,
            PurchaseItemLineQuantity:lineLevelData.Quantity,
            PurchaseItemLineRate:lineLevelData.Rate,
            PurchaseItemLineAmount:lineLevelData.Amount,
            PurchaseItemLineLocationRef:providerInvoice.ServiceStateName,
            // optionals
            HoldReason:providerInvoice.Payment.IsPaidByCreditCard           // specifically driven by CC payment, not any prepayment
                ? NonEmptyText.NewOptionUnvalidated("Paid by credit card") 
                : Option<NonEmptyText>.None,
            HoldChangeDate:providerInvoice.Payment.IsPaidByCreditCard       // specifically driven by CC payment, not any prepayment
                ? Option<DateTimeOffset>.Some(DateTimeOffset.Now)
                : Option<DateTimeOffset>.None,
            PaymentMethod:Option<NonEmptyText>.None, // TODO expect to eventually have a source for this
            ProviderInvoiceNumber:providerInvoice.ProviderInvoiceNumber,
            PurchaseItemLineDescription:lineLevelData.Description,
            PurchaseItemLineReference:purchaseItemLineReference, 
            RateTypeText:lineLevelData.RateTypeText,
            lineLevelData.LaborQuantityInMinutes,
            Memo:Option<NonEmptyText>.None); // we have no memos yet
    }

    /// Convert RateType enum to the text required by NetSuite for a rate type
    public static NonEmptyText ToRateTypeText(RateType rateType) =>
        rateType switch 
        {
            RateType.Regular => NonEmptyText.NewUnsafe("Regular"),
            RateType.Emergency => NonEmptyText.NewUnsafe("Emergency"),
            RateType.Holiday => NonEmptyText.NewUnsafe("Holiday"),      // TODO may need to change for NetSuite when Holiday rate type is implemented
            // TODO create unit test to prevent this wildcard for occurring
            _ => NonEmptyText.NewUnsafe(rateType.ToString())  
        };
    
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelData(ProviderInvoiceMaterialPartLineItem lineItem) =>
        new (FalItemType.MaterialPart, lineItem.Quantity, lineItem.ItemCost.Value, lineItem.LineItemCost.Value, RateTypeText:Option<NonEmptyText>.None, Option<decimal>.None, lineItem.Item.Name, lineItem.LineReference);
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelData(ProviderInvoiceEquipmentLineItem lineItem) =>
        new (FalItemType.Equipment, lineItem.Quantity, lineItem.ItemCost.Value, lineItem.LineItemCost.Value, RateTypeText:Option<NonEmptyText>.None, Option<decimal>.None, lineItem.Item.Name, lineItem.LineReference);
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelData(ProviderInvoiceLaborLineItem lineItem) =>
        new (FalItemType.Labor, lineItem.Hours, lineItem.LaborRate.Value, lineItem.LineItemCost.Value, RateTypeText:Option<NonEmptyText>.Some(ToRateTypeText(lineItem.RateType)), Some(DateTimeUtility.ConvertHoursToMinutes(lineItem.Hours)), lineItem.Description, lineItem.LineReference);
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelData(ProviderInvoiceTripChargeLineItem lineItem) =>
        new(FalItemType.Trip, 1, lineItem.TripChargeRate.Value, lineItem.LineItemCost.Value, RateTypeText:Option<NonEmptyText>.Some(ToRateTypeText(lineItem.RateType)), Option<decimal>.None, lineItem.Description, lineItem.LineReference);
    private static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelData(ProviderInvoiceJobFlatRateLineItem lineItem) =>
        new (FalItemType.FlatRateJob,
            lineItem.Quantity, 
            lineItem.Rate, 
            lineItem.LineItemCost,
            RateTypeText:Option<NonEmptyText>.None,
            Option<decimal>.None,
            Option<NonEmptyText>.Some(lineItem.ServiceTypeName),
            Option<NonEmptyText>.Some(lineItem.LineReference));
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelDataCostDiscount(ProviderInvoiceCostDiscountLineItem lineItem, FalItemType falItemType) =>
        new (falItemType, 1, lineItem.CostDiscount, lineItem.CostDiscount, RateTypeText:Option<NonEmptyText>.None, Option<decimal>.None, lineItem.Description, lineItem.LineReference);
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelDataCostDiscountFlatRate(ProviderInvoiceCostDiscountLineItem lineItem) =>
        // TODO Adapt this to distinguish between job and catalog after Fulfillment provides more data on the cost adjustment.
        new (FalItemType.FlatRateJob, 1, lineItem.CostDiscount, lineItem.CostDiscount, RateTypeText:Option<NonEmptyText>.None, Option<decimal>.None, lineItem.Description, lineItem.LineReference);
    public static VendorBillLineInsertLineLevelData ToVendorBillLineInsertLineLevelData(ProviderInvoiceProcessingFeeLineItem lineItem) =>
        new (FalItemType.ProcessingFee, lineItem.Quantity, lineItem.ProcessingFee.Value, lineItem.LineProcessingFee.Value, RateTypeText:Option<NonEmptyText>.None, Option<decimal>.None, lineItem.Description, lineItem.LineReference);

    /// Map a provider invoice to a list of vendor bill lines for insert
    private static Lst<VendorBillLineInsert> ToVendorBillLineInsertsTimeAndMaterial(ProviderInvoiceInsert providerInvoice) =>
        Lst<VendorBillLineInsert>.Empty
            // Append order matters
            .Append(providerInvoice.MaterialPartLineItems.Map(providerInvoiceMaterialPartLineItem => 
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(providerInvoiceMaterialPartLineItem))))
            .Append(providerInvoice.EquipmentLineItems.Map(providerInvoiceEquipmentLineItem => 
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(providerInvoiceEquipmentLineItem))))
            .Append(providerInvoice.LaborLineItems
                .Filter(providerInvoiceLaborLineItem => providerInvoiceLaborLineItem.Hours > 0)   // A labor line item with 0.0 hours should not be sent to FAL. TODO Check LineItemCost instead to account for 0.0 rate and a >0.0 hours
                .Map(providerInvoiceLaborLineItemWithHours => 
                    ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(providerInvoiceLaborLineItemWithHours))))
            .Append(providerInvoice.TripChargeLineItems.Map(providerInvoiceTripChargeLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(providerInvoiceTripChargeLineItem))))
            .Append(providerInvoice.ProcessingFeeLineItems.Map(providerInvoiceProcessingFeeLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(providerInvoiceProcessingFeeLineItem))))
            .Append(providerInvoice.MaterialCostDiscountLineItems.Map(providerInvoiceCostDiscountLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelDataCostDiscount(providerInvoiceCostDiscountLineItem, FalItemType.MaterialPart))))
            .Append(providerInvoice.EquipmentCostDiscountLineItems.Map(providerInvoiceCostDiscountLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelDataCostDiscount(providerInvoiceCostDiscountLineItem, FalItemType.Equipment))))
            .Append(providerInvoice.LaborCostDiscountLineItems.Map(providerInvoiceCostDiscountLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelDataCostDiscount(providerInvoiceCostDiscountLineItem, FalItemType.Labor))));
    
    private static Lst<VendorBillLineInsert> ToVendorBillLineInsertsFlatRate(ProviderInvoiceInsert providerInvoice) =>
        // Append order matters
        Lst<VendorBillLineInsert>.Empty
            .Append(providerInvoice.JobFlatRateLineItems.Map(jobFlatRateLineItem => 
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(jobFlatRateLineItem))))
            // The determination of a processing fee for flat rate costing is driven only by the domain rule JobBillingRule.BuildProcessingFeeSection.
            // This append will only add processing fees if they have been created on the invoice due to the rule.
            .Append(providerInvoice.ProcessingFeeLineItems.Map(providerInvoiceProcessingFeeLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelData(providerInvoiceProcessingFeeLineItem))))
            .Append(providerInvoice.JobFlatRateCostDiscountLineItems.Map(providerInvoiceCostDiscountLineItem =>
                ToVendorBillLineInsert(providerInvoice, ToVendorBillLineInsertLineLevelDataCostDiscountFlatRate(providerInvoiceCostDiscountLineItem))));

    public static Lst<VendorBillLineInsert> ToVendorBillLineInserts(ProviderInvoiceInsert providerInvoice) =>
        ProviderInvoiceRule.IsFlatRateCosted(providerInvoice)
            ? ToVendorBillLineInsertsFlatRate(providerInvoice)
            : ToVendorBillLineInsertsTimeAndMaterial(providerInvoice);
    
    public static Validation<ErrorMessage, Lst<VendorBillLineInsert>> ToVendorBillLineInserts(ProviderInvoiceInsert providerInvoice, JobBillingId jobBillingId) =>
        VendorBillValidator.Validate(ToVendorBillLineInserts(providerInvoice), jobBillingId);
}