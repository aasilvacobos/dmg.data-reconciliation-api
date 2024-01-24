using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between catalog protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class CatalogItemMessageMapper 
{
    public static DT.Domain.UnitOfMeasure ToUnitOfMeasure(DMG.Common.UnitType unitTypeMessage) =>
        unitTypeMessage
            switch 
            {
                DMG.Common.UnitType.Case => DT.Domain.UnitOfMeasure.Case,
                DMG.Common.UnitType.Day => DT.Domain.UnitOfMeasure.Day,
                DMG.Common.UnitType.Gallon => DT.Domain.UnitOfMeasure.Gallon,
                DMG.Common.UnitType.Hour => DT.Domain.UnitOfMeasure.Hour,
                DMG.Common.UnitType.Item => DT.Domain.UnitOfMeasure.Item,
                DMG.Common.UnitType.Kilo => DT.Domain.UnitOfMeasure.Kilo,
                DMG.Common.UnitType.Liter => DT.Domain.UnitOfMeasure.Liter,
                DMG.Common.UnitType.Minute => DT.Domain.UnitOfMeasure.Minute,
                DMG.Common.UnitType.Pound => DT.Domain.UnitOfMeasure.Pound,
                DMG.Common.UnitType.Ton => DT.Domain.UnitOfMeasure.Ton,
                DMG.Common.UnitType.Trip => DT.Domain.UnitOfMeasure.Trip,
                DMG.Common.UnitType.Week => DT.Domain.UnitOfMeasure.Week,
                DMG.Common.UnitType.Yd2 => DT.Domain.UnitOfMeasure.Yd2,
                DMG.Common.UnitType.Yd3 => DT.Domain.UnitOfMeasure.Yd3,
                DMG.Common.UnitType.Mile => DT.Domain.UnitOfMeasure.Mile,
                DMG.Common.UnitType.Meter => DT.Domain.UnitOfMeasure.Meter,
                DMG.Common.UnitType.Job => DT.Domain.UnitOfMeasure.Job,
                DMG.Common.UnitType.Foot => DT.Domain.UnitOfMeasure.Foot,
                DMG.Common.UnitType.Inch => DT.Domain.UnitOfMeasure.Inch,
                DMG.Common.UnitType.Event => DT.Domain.UnitOfMeasure.Event,
                DMG.Common.UnitType.Truck => DT.Domain.UnitOfMeasure.Truck,
                DMG.Common.UnitType.Bag => DT.Domain.UnitOfMeasure.Bag,
                DMG.Common.UnitType.Service => DT.Domain.UnitOfMeasure.Service,
                DMG.Common.UnitType.Unspecified => DT.Domain.UnitOfMeasure.Unspecified,
                DMG.Common.UnitType.Second => DT.Domain.UnitOfMeasure.Second,
                _ => DT.Domain.UnitOfMeasure.Unspecified
            };
    
    public static DT.Domain.CatalogItemType ToCatalogItemType(DMG.Common.CatalogItemType catalogItemTypeMessage) =>
        catalogItemTypeMessage
            switch 
            {
                DMG.Common.CatalogItemType.Material => DT.Domain.CatalogItemType.Material,
                DMG.Common.CatalogItemType.Part => DT.Domain.CatalogItemType.Part,
                DMG.Common.CatalogItemType.OwnedEquipment => DT.Domain.CatalogItemType.EquipmentOwned,
                DMG.Common.CatalogItemType.RentalEquipment => DT.Domain.CatalogItemType.EquipmentRental,
                DMG.Common.CatalogItemType.Labor => DT.Domain.CatalogItemType.Labor,
                DMG.Common.CatalogItemType.Trip => DT.Domain.CatalogItemType.Trip,
                DMG.Common.CatalogItemType.FlatRate => DT.Domain.CatalogItemType.FlatRate,
                DMG.Common.CatalogItemType.Fee => DT.Domain.CatalogItemType.Unspecified,    // Fee is currently not in PI scope
                _ => DT.Domain.CatalogItemType.Unspecified
            };

    public static DT.Domain.CatalogItem ToEntity(DMG.ItemCatalog.CatalogItem catalogItemMessage) => 
        new DT.Domain.CatalogItem(
            new CatalogItemId(ParseGuidStringDefaultToEmptyGuid(catalogItemMessage.CatalogItemId)),
            NonEmptyText.NewUnsafe(catalogItemMessage.Name.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
            ToCatalogItemType(catalogItemMessage.ItemType),
            ToUnitOfMeasure(catalogItemMessage.UnitType), 
            // optionals
            TryParseGuidString(catalogItemMessage.ParentId),
            NonEmptyText.NewOptionUnvalidated(catalogItemMessage.Category),
            NonEmptyText.NewOptionUnvalidated(catalogItemMessage.Description),
            NonEmptyText.NewOptionUnvalidated(catalogItemMessage.Brand),
            NonEmptyText.NewOptionUnvalidated(catalogItemMessage.CommonId),
            NonEmptyText.NewOptionUnvalidated(catalogItemMessage.ManufacturersPartNum),
            NonEmptyText.NewOptionUnvalidated(catalogItemMessage.Upc),
            catalogItemMessage.ImageIds.Freeze()    // use first entry that is not empty
                .Map(NonEmptyText.NewOptionUnvalidated)
                .Somes()
                .ToOption());
}