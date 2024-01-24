namespace DMG.ProviderInvoicing.DT.Domain;

public class ProviderBillingCrudEntity
{
    public record ProviderBillingCrudMutateModel(
        string? ProviderInvoiceNumber,
        string? Note
        // ,ProviderBillingCrudMutateVisitModel VisitCrud,
        // ProviderBillingCrudMutateDiscountModel DiscountCrud,
        // ProviderBillingCrudMutatePhotoModel PhotoCrud,
        // ProviderBillingServiceLineItemModel ServiceLineItem
    );
    
}