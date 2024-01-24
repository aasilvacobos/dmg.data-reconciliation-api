using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.DT.Domain;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.EntityMapper;

/// Provides deterministic mapping from catalog entry entities to Protobuf messages. 
public static class CatalogEntryEntityMapper 
{
    public static DMG.Common.CatalogItemType ToMessage(MaterialPartEquipmentCatalogItemType materialPartEquipmentCatalogItemType) =>
        materialPartEquipmentCatalogItemType
            switch 
            {
                MaterialPartEquipmentCatalogItemType.Material => Common.CatalogItemType.Material,
                MaterialPartEquipmentCatalogItemType.Part => Common.CatalogItemType.Part,
                MaterialPartEquipmentCatalogItemType.EquipmentOwned => Common.CatalogItemType.OwnedEquipment,
                MaterialPartEquipmentCatalogItemType.EquipmentRental => Common.CatalogItemType.RentalEquipment,
                 _ => Common.CatalogItemType.Unspecified // should be impossible since MaterialPartEquipmentCatalogItemType is limited to 4 values above
            };
}