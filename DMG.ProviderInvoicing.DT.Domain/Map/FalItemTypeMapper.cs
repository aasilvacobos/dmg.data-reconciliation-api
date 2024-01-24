using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain.Map;

public static class FalItemTypeMapper
{
    public static NonEmptyText ToLineReference(FalItemType falItemType) =>
        falItemType switch 
        {
            FalItemType.MaterialPart => NonEmptyText.NewUnsafe("Parts & Materials"),
            FalItemType.Equipment => NonEmptyText.NewUnsafe("Equipment"),
            FalItemType.Labor => NonEmptyText.NewUnsafe("Labor"),
            FalItemType.RoutineLabor => NonEmptyText.NewUnsafe("Labor"),
            FalItemType.Trip => NonEmptyText.NewUnsafe("Trip Charges"),
            FalItemType.FlatRateJob => NonEmptyText.NewUnsafe("Flat Rate Labor"),
            FalItemType.ProcessingFee => NonEmptyText.NewUnsafe("Processing Fee"),
            _ => NonEmptyText.NewUnsafe("Undefined") 
        };

    public static NonEmptyText ToDiscountLineDescription(FalItemType falItemType) =>
        falItemType switch 
        {
            FalItemType.MaterialPart => NonEmptyText.NewUnsafe("Material Discount"),
            FalItemType.Equipment => NonEmptyText.NewUnsafe("Equipment Discount"),
            FalItemType.Labor => NonEmptyText.NewUnsafe("Labor Discount"),
            FalItemType.RoutineLabor => NonEmptyText.NewUnsafe("Labor Discount"),
            FalItemType.Trip => NonEmptyText.NewUnsafe("N/A"),
            FalItemType.FlatRateJob => NonEmptyText.NewUnsafe("Flat Rate Labor Discount"),
            FalItemType.ProcessingFee => NonEmptyText.NewUnsafe("N/A"),
            _ => NonEmptyText.NewUnsafe("Undefined") 
        };
    
    public static FalItemType OfMaterialPartEquipmentType (MaterialPartEquipmentCatalogItemType itemType) =>
        itemType switch 
        {
            MaterialPartEquipmentCatalogItemType.Material => FalItemType.MaterialPart,
            MaterialPartEquipmentCatalogItemType.Part => FalItemType.MaterialPart,
            MaterialPartEquipmentCatalogItemType.EquipmentRental => FalItemType.Equipment,
            MaterialPartEquipmentCatalogItemType.EquipmentOwned => FalItemType.Equipment,
            _ => FalItemType.MaterialPart 
        };
}