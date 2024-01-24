namespace DMG.ProviderInvoicing.DT.Domain.Map;

public static class CatalogItemTypeMapper 
{
    public static MaterialPartCatalogItemType ToMaterialPartCatalogItemType(MaterialPartEquipmentCatalogItemType materialPartEquipmentCatalogItemType) =>
        materialPartEquipmentCatalogItemType switch {
            MaterialPartEquipmentCatalogItemType.Material => MaterialPartCatalogItemType.Material,
            MaterialPartEquipmentCatalogItemType.Part => MaterialPartCatalogItemType.Part,
            _ => MaterialPartCatalogItemType.Material };
    public static EquipmentCatalogItemType ToEquipmentCatalogItemType(MaterialPartEquipmentCatalogItemType materialPartEquipmentCatalogItemType) =>
        materialPartEquipmentCatalogItemType switch {
            MaterialPartEquipmentCatalogItemType.EquipmentOwned => EquipmentCatalogItemType.EquipmentOwned,
            MaterialPartEquipmentCatalogItemType.EquipmentRental => EquipmentCatalogItemType.EquipmentRental,
            _ => EquipmentCatalogItemType.EquipmentRental };
    public static MaterialPartEquipmentCatalogItemType ToMaterialPartEquipmentCatalogItemType(MaterialPartCatalogItemType materialPartCatalogItemType) =>
        materialPartCatalogItemType switch 
        {
            MaterialPartCatalogItemType.Material => MaterialPartEquipmentCatalogItemType.Material,
            MaterialPartCatalogItemType.Part => MaterialPartEquipmentCatalogItemType.Part,
            _ => MaterialPartEquipmentCatalogItemType.Material  // only possible if new case added to MaterialPartCatalogItemType
        };
    public static MaterialPartEquipmentCatalogItemType ToMaterialPartEquipmentCatalogItemType(EquipmentCatalogItemType equipmentCatalogItemType) =>
        equipmentCatalogItemType switch 
        {
            EquipmentCatalogItemType.EquipmentOwned => MaterialPartEquipmentCatalogItemType.EquipmentOwned,
            EquipmentCatalogItemType.EquipmentRental => MaterialPartEquipmentCatalogItemType.EquipmentRental,
            _ => MaterialPartEquipmentCatalogItemType.EquipmentOwned // only possible if new case added to EquipmentCatalogItemType
        };
}