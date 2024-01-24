using System;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain.Validation; 

/// Validations related to the job billing
public static class JobBillingValidator
{
    /// Validate a single catalog item rule value. Will return validation error(s) for rule name only, rule value only, or both.
    public static Validation<ErrorMessage, JobBillingPatchCatalogItemRuleValue> ValidatePatchMaterialPartEquipmentItemRuleValue(JobBillingPatchUnvalidatedCatalogItemRuleValue unvalidatedRuleValue) 
    {
        var ruleNameValidation = NonEmptyText.New(unvalidatedRuleValue.RuleName)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidatedRuleValue.RuleName)))
            .ToValidation();
        var ruleValueValidation = NonEmptyText.New(unvalidatedRuleValue.RuleValue)
            .MapLeft(_ => ErrorMessage.NewRequiredField(nameof(unvalidatedRuleValue.RuleValue)))
            .ToValidation();
        
        return(ruleNameValidation, ruleValueValidation)
            .Apply((ruleNameValid, ruleValueValid) => 
                new JobBillingPatchCatalogItemRuleValue(
                    ruleNameValid,
                    ruleValueValid));
    }
    
    /// Validate a collection of catalog item rule values. Only return the first validation error.
    public static Validation<ErrorMessage, Lst<JobBillingPatchCatalogItemRuleValue>> ValidatePatchMaterialPartEquipmentItemRuleValues(Lst<JobBillingPatchUnvalidatedCatalogItemRuleValue> unvalidatedRuleValues) => 
        unvalidatedRuleValues
            .Map(ValidatePatchMaterialPartEquipmentItemRuleValue)
            .Traverse(x => x);
    
    public static Validation<ErrorMessage, IJobBillingPatchMaterialPartEquipmentItem> ValidatePatchMaterialPartEquipmentItem(IJobBillingPatchUnvalidatedMaterialPartEquipmentItem iJobBillingPatchUnvalidatedMaterialPartEquipmentItem) =>
        iJobBillingPatchUnvalidatedMaterialPartEquipmentItem switch {
            JobBillingPatchUnvalidatedMaterialPartEquipmentCatalogItem item =>
                ValidatePatchMaterialPartEquipmentItemRuleValues(item.RuleValues)
                    .Map(ruleValues => new JobBillingPatchMaterialPartEquipmentCatalogItem(item.CatalogItemId, ruleValues) as IJobBillingPatchMaterialPartEquipmentItem),
            JobBillingPatchUnvalidatedMaterialPartEquipmentNonCatalogItem item => 
                NonEmptyText.New(item.Name)
                    .Match<Either<ErrorMessage, IJobBillingPatchMaterialPartEquipmentItem>> (
                        Right: nameValid=> Right(new JobBillingPatchMaterialPartEquipmentNonCatalogItem(nameValid, item.CatalogItemType) as IJobBillingPatchMaterialPartEquipmentItem),
                        Left: _ => Left(ErrorMessage.NewRequiredField(nameof(item.Name))))
                    .ToValidation(),
            _ => // valid case to throw because if a new class implements interface, we need to know immediately know that it is not implemented here
                throw new ArgumentException($"Invalid IJobBillingPatchUnvalidatedMaterialPartEquipmentItem item argument {nameof(iJobBillingPatchUnvalidatedMaterialPartEquipmentItem)}.") };

    private static Validation<ErrorMessage, JobBillingPatchMaterialPartLineItem> ValidatePatchMaterialPartLineItem(JobBillingPatchUnvalidatedMaterialPartLineItem unvalidated) =>
        ValidatePatchMaterialPartEquipmentItem(unvalidated.Item)
            .Map(itemValid => new JobBillingPatchMaterialPartLineItem(
                Optional(unvalidated.Id).IfNone(Guid.NewGuid), // create new id for insert
                itemValid,
                unvalidated.Quantity,
                unvalidated.UnitType,
                unvalidated.ItemCost));

    private static Validation<ErrorMessage, JobBillingPatchEquipmentLineItem> ValidatePatchEquipmentLineItem(JobBillingPatchUnvalidatedEquipmentLineItem unvalidated) =>
        ValidatePatchMaterialPartEquipmentItem(unvalidated.Item)
            .Map(itemValid => new JobBillingPatchEquipmentLineItem(
                Optional(unvalidated.Id).IfNone(Guid.NewGuid), // create new id for insert 
                itemValid,
                unvalidated.Quantity,
                unvalidated.UnitType,
                unvalidated.ItemCost));

    public static Validation<ErrorMessage, JobBillingPatch> ValidatePatch(JobBillingPatchUnvalidated unvalidated) 
    {
        var jobSummaryOptionValidation = NonEmptyText.NewOption(unvalidated.JobSummary)
            .MapLeft(_ => ErrorMessage.NewStringIsEmptyOrWhiteSpace(nameof(unvalidated.JobSummary)))
            .ToValidation();
        var providerInvoiceNumberOptionValidation = NonEmptyText.NewOption(unvalidated.ProviderInvoiceNumber)
            .MapLeft(_ => ErrorMessage.NewStringIsEmptyOrWhiteSpace(nameof(unvalidated.ProviderInvoiceNumber)))
            .ToValidation();
        var materialPartLinesItemsValidation = unvalidated.MaterialPartItems.Map(ValidatePatchMaterialPartLineItem)
            .Traverse(x => x);
        var equipmentLinesItemsValidation = unvalidated.EquipmentItems.Map(ValidatePatchEquipmentLineItem)
            .Traverse(x => x);
        
        return(jobSummaryOptionValidation, providerInvoiceNumberOptionValidation, materialPartLinesItemsValidation, equipmentLinesItemsValidation)
            .Apply((jobSummaryOptionValid, providerInvoiceNumberOptionValid, materialPartLinesItemsValid, equipmentLinesItemsValid) => 
                new JobBillingPatch(unvalidated.JobBillingId,
                                    unvalidated.Version,
                                    unvalidated.Meta,
                                    jobSummaryOptionValid,
                                    providerInvoiceNumberOptionValid,
                                    materialPartLinesItemsValid,
                                    equipmentLinesItemsValid));
    }
    
    public static Validation<ErrorMessage, JobBillingSubmitForInvoicing> ValidateSubmitForInvoicing(JobBillingSubmitForInvoicingUnvalidated unvalidated) =>
        new JobBillingSubmitForInvoicing(unvalidated.JobBillingId,unvalidated.ProviderBillingId, unvalidated.Meta);
}